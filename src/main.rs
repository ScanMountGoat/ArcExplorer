use std::{borrow::Cow, sync::LazyLock};

use directories::ProjectDirs;
use gpui_kit::{
    App, AssetSource, Bounds, Entity, EventEmitter, Focusable, KeyBinding, MouseButton,
    NavigationDirection, Pixels, Result, SharedString, Subscription, TitlebarOptions, Window,
    WindowBounds, WindowKind, WindowOptions, actions, application,
    assets::{Assets, IconName, icon_assets},
    base::{
        FocusableExt, IndexPath, h_flex, h_resizable, input::InputBaseState, resizable_panel,
        v_flex,
    },
    component::{
        ActiveTheme, Icon, Sizable, Theme, ThemeRegistry,
        button::{Button, ButtonVariants},
        checkbox::Checkbox,
        form::{field, h_form},
        input::{Input, InputEvent, InputMode, InputState},
        label::Label,
        list::{List, ListDelegate, ListItem, ListState},
        menu::{DropdownMenu, PopupMenu},
        select::{Select, SelectEvent, SelectItem, SelectState},
        status_bar::StatusBar,
        table::{Column, ColumnSort, DataTable, TableDelegate, TableEvent, TableState},
    },
    div, init, open_window,
    prelude::*,
    px, size,
};
use rfd::AsyncFileDialog;
use smash_arc::Hash40;

use crate::{
    arc::{ArcData, FileNodeInfo, arc_file_count, arc_version, load_files, search_arc},
    path_bar::PathBar,
    preferences::ApplicationPreferences,
};

mod arc;
mod path_bar;
mod preferences;

// Add any extra Lucide icons not included in the base icon set.
icon_assets!(ExtraIcons, [Link]);

struct AppAssets;

impl AssetSource for AppAssets {
    fn load(&self, path: &str) -> Result<Option<Cow<'static, [u8]>>> {
        if let Some(bytes) = ExtraIcons.load(path)? {
            return Ok(Some(bytes));
        }
        Assets.load(path)
    }

    fn list(&self, path: &str) -> Result<Vec<SharedString>> {
        let mut paths = Assets.list(path)?;
        paths.extend(ExtraIcons.list(path)?);
        paths.sort();
        paths.dedup();
        Ok(paths)
    }
}

pub static PROJECT_DIR: LazyLock<ProjectDirs> = LazyLock::new(|| {
    // TODO: Avoid unwrap.
    ProjectDirs::from("", "", "ArcExplorer").unwrap()
});

fn application_directory() -> std::path::PathBuf {
    // TODO: Find a better way to strip the /data on Windows.
    let data_local_dir = PROJECT_DIR.data_local_dir();
    if data_local_dir.ends_with("data") {
        data_local_dir.parent().unwrap_or(data_local_dir).to_owned()
    } else {
        data_local_dir.to_owned()
    }
}

struct ArcFilesTableDelegate {
    files: Vec<FileNodeInfo>,
    columns: Vec<Column>,
}

impl ArcFilesTableDelegate {
    fn new(files: Vec<FileNodeInfo>) -> Self {
        Self {
            files,
            columns: vec![
                Column::new("name", "Name").sortable().ascending(),
                Column::new("description", "Description")
                    .width(px(200.0))
                    .sortable(),
                Column::new("compressed_size", "Compressed Size")
                    .width(px(200.0))
                    .sortable(),
                Column::new("decompressed_size", "Decompressed Size")
                    .width(px(200.0))
                    .sortable(),
            ],
        }
    }
}

impl TableDelegate for ArcFilesTableDelegate {
    fn columns_count(&self, _cx: &App) -> usize {
        self.columns.len()
    }

    fn rows_count(&self, _: &App) -> usize {
        self.files.len()
    }

    fn column(&self, col_ix: usize, _: &App) -> Column {
        self.columns[col_ix].clone()
    }

    fn render_td(
        &mut self,
        row_ix: usize,
        col_ix: usize,
        _: &mut Window,
        _: &mut Context<TableState<Self>>,
    ) -> impl IntoElement {
        let row = &self.files[row_ix];
        let col = &self.columns[col_ix];

        // TODO: manually set background colors without opacity
        match col.key.as_ref() {
            "name" => h_flex()
                .gap(px(10.0))
                .child(Icon::new(IconName::FolderClosed))
                .child(row.name().to_string())
                .when(row.is_regional(), |this| {
                    this.child(Icon::new(IconName::Globe))
                })
                .when(row.is_shared(), |this| {
                    this.child(Icon::new(IconName::Link))
                })
                .into_any_element(),
            "description" => row.description().to_string().into_any_element(),
            // TODO: where to handle size formatting?
            // TODO: port formatting from arcexplorer with tests
            "compressed_size" => row.compressed_size().to_string().into_any_element(),
            "decompressed_size" => row.decompressed_size().to_string().into_any_element(),
            _ => div().into_any_element(),
        }
    }

    fn render_empty(
        &mut self,
        _window: &mut Window,
        _cx: &mut Context<TableState<Self>>,
    ) -> impl IntoElement {
        div()
    }

    // TODO: test sorting for all sortable columns
    fn perform_sort(
        &mut self,
        col_ix: usize,
        sort: ColumnSort,
        _window: &mut Window,
        cx: &mut Context<TableState<Self>>,
    ) {
        // TODO: sorting function
        // TODO: avoid clone
        // TODO: sort sizes by integer values and not string comparison.
        // TODO store default order for disabled sorting?
        match self.columns[col_ix].key.as_ref() {
            "name" => match sort {
                // TODO: should the default order by alphabetical to match ArcExplorer?
                ColumnSort::Default => {
                    // TODO: Restore default order to not break search
                    self.files.sort_by_key(|f| f.name().to_string());
                }
                ColumnSort::Ascending => {
                    self.files.sort_by_key(|f| f.name().to_string());
                }
                ColumnSort::Descending => {
                    self.files.sort_by_key(|f| f.name().to_string());
                    self.files.reverse();
                }
            },
            "description" => match sort {
                ColumnSort::Default => (),
                ColumnSort::Ascending => {
                    self.files.sort_by_key(|f| f.description().to_string());
                }
                ColumnSort::Descending => {
                    self.files.sort_by_key(|f| f.description().to_string());
                    self.files.reverse();
                }
            },
            "compressed_size" => match sort {
                ColumnSort::Default => (),
                ColumnSort::Ascending => {
                    self.files.sort_by_key(|f| f.compressed_size().to_string());
                }
                ColumnSort::Descending => {
                    self.files.sort_by_key(|f| f.compressed_size().to_string());
                    self.files.reverse();
                }
            },
            "decompressed_size" => match sort {
                ColumnSort::Default => (),
                ColumnSort::Ascending => {
                    self.files
                        .sort_by_key(|f| f.decompressed_size().to_string());
                }
                ColumnSort::Descending => {
                    self.files
                        .sort_by_key(|f| f.decompressed_size().to_string());
                    self.files.reverse();
                }
            },
            _ => (),
        }
        cx.notify();
    }

    fn context_menu(
        &mut self,
        _row_ix: usize,
        menu: PopupMenu,
        _window: &mut Window,
        _cx: &mut Context<TableState<Self>>,
    ) -> PopupMenu {
        // TODO: extract files if folder?
        menu.menu("Extract File", Box::new(TableExtractFile))
            .menu("Open Parent Folder", Box::new(TableOpenParentFolder))
    }
}

#[derive(Debug, Clone)]
struct ArcRegion(smash_arc::Region);

impl SelectItem for ArcRegion {
    type Value = smash_arc::Region;

    fn title(&self) -> SharedString {
        match self.0 {
            smash_arc::Region::None => "None".into(),
            smash_arc::Region::Japanese => "Japanese".into(),
            smash_arc::Region::UsEnglish => "English (US)".into(),
            smash_arc::Region::UsFrench => "French (US)".into(),
            smash_arc::Region::UsSpanish => "Spanish (US)".into(),
            smash_arc::Region::EuEnglish => "English (EU)".into(),
            smash_arc::Region::EuFrench => "French (EU)".into(),
            smash_arc::Region::EuSpanish => "Spanish (EU)".into(),
            smash_arc::Region::EuGerman => "German (EU)".into(),
            smash_arc::Region::EuDutch => "Dutch (EU)".into(),
            smash_arc::Region::EuItalian => "Italian (EU)".into(),
            smash_arc::Region::EuRussian => "Russian (EU)".into(),
            smash_arc::Region::Korean => "Korean".into(),
            smash_arc::Region::ChinaChinese => "Chinese (China)".into(),
            smash_arc::Region::TaiwanChinese => "Chinese (Taiwan)".into(),
        }
    }

    fn value(&self) -> &Self::Value {
        &self.0
    }
}

actions!(
    arc_explorer,
    [
        OpenArc,
        ConnectToArc,
        Extract,
        ExtractAll,
        OpenPreferences,
        TableEnterFolder,
        TableExitFolder,
        TableExtractFile,
        TableOpenParentFolder,
        Quit
    ]
);

struct Application {
    arc: Entity<Option<ArcData>>,

    preferences: Entity<ApplicationPreferences>,

    path_bar: Entity<PathBar>,

    details_panel: Entity<FileDetailsPanel>,

    table: Entity<TableState<ArcFilesTableDelegate>>,
    region_select: Entity<SelectState<Vec<ArcRegion>>>,
    search_input: Entity<InputBaseState<InputMode>>,

    _subscriptions: Vec<Subscription>,
}

impl Application {
    fn new(
        window: &mut Window,
        cx: &mut Context<Self>,
        preferences: ApplicationPreferences,
    ) -> Self {
        // TODO: where should hashes be loaded?
        // TODO: load hashes from a separate thread?
        // TODO: download hashes or hash update

        let data_local_dir = application_directory();
        let start = std::time::Instant::now();
        Hash40::set_global_labels_file(data_local_dir.join("Hashes.txt")).unwrap();
        println!("Load hashes: {:?}", start.elapsed());

        // TODO: log errors.
        let start = std::time::Instant::now();
        let arc = cx.new(|_cx| {
            preferences
                .arc_startup_location
                .as_ref()
                .map(|path| ArcData::from_path(path, preferences.arc_region))
        });
        println!("Load ARC: {:?}", start.elapsed());

        let preferences = cx.new(|_cx| preferences);

        initialize_keybindings(cx);

        let files = arc
            .read(cx)
            .as_ref()
            .and_then(|arc| {
                load_files(
                    &arc.arc,
                    arc::ROOT_PATH,
                    true,
                    preferences.read(cx).arc_region,
                )
            })
            .unwrap_or_default();

        let delegate = ArcFilesTableDelegate::new(files);
        let table = cx.new(|cx| {
            TableState::new(delegate, window, cx)
                .sortable(true)
                .col_movable(false)
                .col_resizable(false)
                .row_selectable(true)
                .col_selectable(false)
                .cell_selectable(false)
        });
        let search_input = cx.new(|cx| InputState::new(window, cx).placeholder("Search ARC"));

        let search_input_sub =
            cx.subscribe_in(&search_input, window, |this, state, event, window, cx| {
                if let InputEvent::Change = event {
                    let search_term = state.read(cx).value();
                    if !search_term.is_empty() {
                        this.search_files(search_term.as_str(), cx);
                    } else {
                        // No search term should restore the files prior to searching.
                        let folder_path = this.path_bar.read(cx).current_path.to_string();
                        this.change_folder(cx, window, folder_path.clone());
                    }
                }
            });

        let table_sub = cx.subscribe_in(&table, window, |this, table, event, window, cx| {
            match event {
                TableEvent::SelectRow(index) => {
                    let delegate = table.read(cx).delegate();
                    if let Some(file) = delegate.files.get(*index).cloned() {
                        // TODO: Just load this when loading the file itself?
                        let shared_files = this
                            .arc
                            .read(cx)
                            .as_ref()
                            .map(|arc| file.shared_files(arc, this.preferences.read(cx).arc_region))
                            .unwrap_or_default();

                        this.details_panel.update(cx, |this, cx| {
                            this.file.write(cx, Some(file));
                            this.shared_file_list.update(cx, |this, cx| {
                                this.delegate_mut().items = shared_files;
                                cx.notify();
                            });
                            cx.notify();
                        });

                        cx.notify();
                    }
                }
                TableEvent::DoubleClickedRow(index) => {
                    // Load the files from the selected folder.
                    // TODO: make this async?
                    let delegate = table.read(cx).delegate();
                    if let Some(file) = delegate.files.get(*index) {
                        // Check the type to avoid trying to enter files.
                        if let FileNodeInfo::Folder { path, .. } = file {
                            let clicked_path = path.to_string();
                            this.change_folder(cx, window, clicked_path);
                        }
                    }
                }
                _ => (),
            }
        });

        let region_select = cx.new(|cx| -> SelectState<Vec<ArcRegion>> {
            let regions = vec![
                ArcRegion(smash_arc::Region::Japanese),
                ArcRegion(smash_arc::Region::UsEnglish),
                ArcRegion(smash_arc::Region::UsFrench),
                ArcRegion(smash_arc::Region::UsSpanish),
                ArcRegion(smash_arc::Region::EuEnglish),
                ArcRegion(smash_arc::Region::EuFrench),
                ArcRegion(smash_arc::Region::EuSpanish),
                ArcRegion(smash_arc::Region::EuGerman),
                ArcRegion(smash_arc::Region::EuDutch),
                ArcRegion(smash_arc::Region::EuItalian),
                ArcRegion(smash_arc::Region::EuRussian),
                ArcRegion(smash_arc::Region::Korean),
                ArcRegion(smash_arc::Region::ChinaChinese),
                ArcRegion(smash_arc::Region::TaiwanChinese),
            ];
            let selected = preferences.read(cx).arc_region;
            let selected_index = regions.iter().position(|r| r.0 == selected).unwrap_or(1);
            SelectState::new(regions, Some(IndexPath::new(selected_index)), window, cx)
        });
        let region_select_sub = cx.subscribe_in(
            &region_select,
            window,
            |this, _select, event, _window, cx| {
                if let SelectEvent::Confirm(Some(value)) = event {
                    this.preferences
                        .update(cx, |state, _cx| state.arc_region = *value);
                }
            },
        );

        let details_panel = cx.new(|cx| FileDetailsPanel::new(window, cx));

        let path_bar = cx.new(|cx| PathBar::new(arc::ROOT_PATH.into(), window, cx));

        let path_bar_sub = cx.observe_in(&path_bar, window, |this, target, window, cx| {
            let value = target.read(cx).current_path.to_string();
            this.change_folder(cx, window, value);
        });

        Self {
            arc,
            preferences,
            path_bar,
            details_panel,
            table,
            region_select,
            search_input,
            _subscriptions: vec![table_sub, search_input_sub, region_select_sub, path_bar_sub],
        }
    }

    fn change_folder(
        &mut self,
        cx: &mut Context<Application>,
        _window: &mut Window,
        folder_path: String,
    ) {
        // TODO: normalize paths (add to arc module with tests)
        // TODO: is loading no files on invalid paths the most reasonable behavior?
        // TODO: remove leading slash?
        // TODO: visual error indication if no entry for path?
        let cleaned_path = folder_path.replace("\\", "/").replace("//", "/");

        let new_files = self
            .arc
            .read(cx)
            .as_ref()
            .and_then(|arc| {
                load_files(
                    &arc.arc,
                    cleaned_path.as_str(),
                    self.preferences.read(cx).merge_trailing_slash,
                    self.preferences.read(cx).arc_region,
                )
            })
            .unwrap_or_default();

        self.path_bar.update(cx, |state, cx| {
            // Track changes to prevent infinite loops.
            if state.current_path != folder_path {
                state.current_path = folder_path.clone().into();
                cx.notify();
            }
        });

        self.table.update(cx, |state, cx| {
            state.delegate_mut().files = new_files;
            // Always select the first row since the previous folder selection may not be valid.
            state.set_selected_row(0, cx);
            cx.notify();
        });

        cx.notify();
    }

    fn search_files(&mut self, search_term: &str, cx: &mut Context<Application>) {
        // TODO: does this need to be debounced and/or made async?
        if let Some(arc) = self.arc.read(cx).as_ref() {
            let start = std::time::Instant::now();
            let new_files = search_arc(
                &arc.arc,
                &arc.search_cache,
                search_term,
                self.preferences.read(cx).arc_region,
            );
            println!("Search: {:?}", start.elapsed());

            self.table.update(cx, |state, _cx| {
                state.delegate_mut().files = new_files;
            });

            cx.notify();
        }
    }

    fn exit_folder(&mut self, window: &mut Window, cx: &mut Context<'_, Application>) {
        // Change to the parent folder.
        let current_path = self.path_bar.read(cx).current_path.to_string();
        if let Some(parent) = std::path::Path::new(&current_path).parent()
            && !parent.is_empty()
        {
            let new_folder = parent.to_string_lossy().to_string();
            self.change_folder(cx, window, new_folder);
        } else {
            self.change_folder(cx, window, arc::ROOT_PATH.to_string());
        }

        // Select the current file in the parent for easier navigation.
        self.table.update(cx, |state, cx| {
            for (i, row) in state.delegate().files.iter().enumerate() {
                if row.path() == current_path {
                    state.set_selected_row(i, cx);
                    cx.notify();
                    break;
                }
            }
        });
    }

    fn on_action_open_arc(&mut self, _: &OpenArc, window: &mut Window, cx: &mut Context<Self>) {
        cx.spawn_in(window, async move |this, cx| {
            // Await the native OS file picker without blocking the UI thread
            let file_handle = AsyncFileDialog::new()
                .set_title("Open")
                .add_filter("ARC", &["arc"])
                .pick_file()
                .await;

            if let Some(handle) = file_handle {
                let path = handle.path().to_path_buf();

                // Update view state back on the UI context and notify for re-render
                this.update_in(cx, |view, window, cx| {
                    // TODO: report errors.
                    let new_arc = ArcData::from_path(path, view.preferences.read(cx).arc_region);

                    view.arc.write(cx, Some(new_arc));
                    view.change_folder(cx, window, arc::ROOT_PATH.to_string());
                    cx.notify();
                })
                .ok();
            }
        })
        .detach();
    }

    fn on_action_connect_to_arc(
        &mut self,
        _: &ConnectToArc,
        window: &mut Window,
        cx: &mut Context<Self>,
    ) {
        // TODO: closing parent window should close this window too
        let bounds = Bounds::centered(None, size(px(500.0), px(150.0)), cx);
        let options = WindowOptions {
            window_bounds: Some(WindowBounds::Windowed(bounds)),
            titlebar: Some(TitlebarOptions {
                title: Some("Connect to ARC".into()),
                ..Default::default()
            }),
            kind: WindowKind::Dialog,
            ..Default::default()
        };
        let (_, child_window) = open_window(options, cx, |window, cx| {
            cx.new(|cx| ConnectToArcWindow::new(window, cx))
        })
        .unwrap();

        cx.subscribe_in(
            &child_window,
            window,
            |this, _, event: &ConnectToArcInfo, window, cx| {
                // TODO: make this async
                // Update view state back on the UI context and notify for re-render
                // TODO: report errors.
                let new_arc = ArcData::from_ip_address(
                    event.ip_address.as_str(),
                    this.preferences.read(cx).arc_region,
                );

                this.arc.write(cx, Some(new_arc));
                this.change_folder(cx, window, arc::ROOT_PATH.to_string());
                cx.notify();
            },
        )
        .detach();
    }

    fn on_action_open_preferences(
        &mut self,
        _: &OpenPreferences,
        _window: &mut Window,
        cx: &mut Context<Self>,
    ) {
        // TODO: closing parent window should close this window too
        let bounds = Bounds::centered(None, size(px(700.0), px(500.0)), cx);
        let options = WindowOptions {
            window_bounds: Some(WindowBounds::Windowed(bounds)),
            titlebar: Some(TitlebarOptions {
                title: Some("Preferences".into()),
                ..Default::default()
            }),
            ..Default::default()
        };
        // TODO: save preferences on close
        open_window(options, cx, |window, cx| {
            cx.new(|cx| PreferencesWindow::new(window, cx, self.preferences.clone()))
        })
        .unwrap();
    }

    fn on_action_table_enter_folder(
        &mut self,
        _: &TableEnterFolder,
        window: &mut Window,
        cx: &mut Context<Self>,
    ) {
        let state = self.table.read(cx);
        if let Some(index) = state.selected_row()
            && let Some(file) = state.delegate().files.get(index)
        {
            // Check the type to avoid trying to enter files.
            if let FileNodeInfo::Folder { path, .. } = file {
                self.change_folder(cx, window, path.to_string());
            }
        }
    }

    fn on_action_table_exit_folder(
        &mut self,
        _: &TableExitFolder,
        window: &mut Window,
        cx: &mut Context<Self>,
    ) {
        self.exit_folder(window, cx);
    }

    fn on_action_table_extract_file(
        &mut self,
        _: &TableExtractFile,
        _window: &mut Window,
        cx: &mut Context<Self>,
    ) {
        let state = self.table.read(cx);
        if let Some(index) = state.selected_row()
            && let Some(file) = state.delegate().files.get(index)
        {
            // TODO: don't enter into file paths
            let _clicked_path = file.path().to_string();
            // self.change_folder(cx, window, clicked_path);
        }
    }

    fn on_action_table_open_parent_folder(
        &mut self,
        _: &TableOpenParentFolder,
        window: &mut Window,
        cx: &mut Context<Self>,
    ) {
        let state = self.table.read(cx);
        if let Some(index) = state.selected_row()
            && let Some(file) = state.delegate().files.get(index)
        {
            let clicked_path = file.path().to_string();
            if let Some(parent) = std::path::Path::new(&clicked_path).parent()
                && !parent.is_empty()
            {
                let new_folder = parent.to_string_lossy().to_string();
                self.change_folder(cx, window, new_folder);
            } else {
                self.change_folder(cx, window, arc::ROOT_PATH.to_string());
            }
        }
    }
}

fn initialize_keybindings(cx: &mut Context<'_, Application>) {
    let is_mac = cx.platform_info().is_mac();
    // TODO: figure out how to make the key context work on datatable (focus issue?)
    cx.bind_keys([
        KeyBinding::new(if is_mac { "cmd-o" } else { "ctrl-o" }, OpenArc, None),
        KeyBinding::new("right", TableEnterFolder, None),
        KeyBinding::new("enter", TableEnterFolder, None),
        KeyBinding::new("cmd-down", TableEnterFolder, None),
        KeyBinding::new("left", TableExitFolder, None),
        KeyBinding::new("alt-up", TableExitFolder, None),
        KeyBinding::new("cmd-up", TableExitFolder, None),
    ]);
}

#[derive(IntoElement)]
struct ApplicationMenu;

impl RenderOnce for ApplicationMenu {
    fn render(self, _window: &mut Window, _cx: &mut App) -> impl IntoElement {
        h_flex()
            .child(
                Button::new("file")
                    .ghost()
                    .label("File")
                    .dropdown_menu(|menu, _window, _cx| {
                        menu.menu("Open Arc", Box::new(OpenArc))
                            .menu("Connect to Arc", Box::new(ConnectToArc))
                    }),
            )
            .child(
                Button::new("arc")
                    .ghost()
                    .label("Arc")
                    .dropdown_menu(|menu, _window, _cx| {
                        menu.menu("Extract Selected File/Folder", Box::new(Extract))
                            .menu("Extract All Files", Box::new(ExtractAll))
                    }),
            )
            .child(
                Button::new("settings")
                    .ghost()
                    .label("Settings")
                    .dropdown_menu(|menu, _window, _cx| {
                        menu.menu("Preferences", Box::new(OpenPreferences))
                    }),
            )
    }
}

#[derive(Default)]
struct SharedFilesListDelegate {
    items: Vec<SharedString>,
    selected_index: Option<IndexPath>,
}

impl ListDelegate for SharedFilesListDelegate {
    type Item = ListItem;

    fn items_count(&self, _section: usize, _cx: &App) -> usize {
        self.items.len()
    }

    fn render_item(
        &mut self,
        ix: IndexPath,
        _window: &mut Window,
        _cx: &mut Context<ListState<Self>>,
    ) -> Option<Self::Item> {
        self.items.get(ix.row).map(|item| {
            ListItem::new(ix)
                .child(Label::new(item).text_sm().truncate())
                .selected(Some(ix) == self.selected_index)
        })
    }

    fn set_selected_index(
        &mut self,
        ix: Option<IndexPath>,
        _window: &mut Window,
        cx: &mut Context<ListState<Self>>,
    ) {
        self.selected_index = ix;
        cx.notify();
    }
}

struct FileDetailsPanel {
    file: Entity<Option<FileNodeInfo>>,
    shared_file_list: Entity<ListState<SharedFilesListDelegate>>,
}

impl FileDetailsPanel {
    fn new(window: &mut Window, cx: &mut Context<Self>) -> Self {
        let file = cx.new(|_cx| None);

        let shared_file_list =
            cx.new(|cx| ListState::new(SharedFilesListDelegate::default(), window, cx));

        Self {
            file,
            shared_file_list,
        }
    }
}

impl Render for FileDetailsPanel {
    fn render(&mut self, _window: &mut Window, cx: &mut Context<Self>) -> impl IntoElement {
        // TODO: how often should this be rerendering?

        // TODO: avoid string cloning?
        // TODO: use the appropriate icon or thumbnail based on file type
        // TODO: This doesn't update when first entering a folder?
        if let Some(file) = self.file.read(cx) {
            v_flex()
                .size_full()
                .gap_5()
                .p_5()
                .child(
                    h_flex()
                        .gap(px(10.0))
                        .child(Icon::new(IconName::FolderClosed).large())
                        .child(div().text_lg().child(file.name().to_string())),
                )
                .child(file.path().to_string())
                .child(match file {
                    FileNodeInfo::Folder { .. } => div().into_any_element(),
                    FileNodeInfo::File {
                        offset,
                        description,
                        is_compressed,
                        is_regional,
                        compressed_size,
                        decompressed_size,
                        ..
                    } => div()
                        .grid()
                        .grid_cols(2)
                        .child("Description")
                        .child(description.clone())
                        .child("Offset")
                        .child(offset.to_string())
                        .child("Compressed")
                        .child(if *is_compressed { "True" } else { "False" })
                        .child("Compressed Size")
                        .child(compressed_size.clone())
                        .child("Decompressed Size")
                        .child(decompressed_size.clone())
                        .when(*is_regional, |this| {
                            this.child(
                                h_flex()
                                    .gap(px(10.0))
                                    .child(Icon::new(IconName::Globe))
                                    .child("Regional"),
                            )
                            .child("True")
                        })
                        .into_any_element(),
                })
                .when(file.is_shared(), |this| {
                    let count = self.shared_file_list.read(cx).delegate().items.len();
                    this.child(
                        h_flex()
                            .gap(px(10.0))
                            .child(Icon::new(IconName::Link))
                            .child(format!("Shared with the following {count} files")),
                    )
                    // TODO: always show scroll bars if needed and not just on hover
                    .child(List::new(&self.shared_file_list).max_h(px(250.0)))
                })
        } else {
            div()
        }
    }
}

fn styled_input<T>(
    id: &'static str,
    input: &Entity<InputBaseState<InputMode>>,
    window: &mut Window,
    cx: &mut Context<T>,
) -> impl IntoElement {
    let is_focused = input.read(cx).focus_handle(cx).is_focused(window);
    let border_color = if is_focused {
        cx.theme().ring
    } else {
        cx.theme().input
    };

    div()
        .id(id)
        .border_1()
        .size_full()
        .border_color(border_color)
        .hover(|style| style.border_color(cx.theme().ring))
        .child(Input::new(input).appearance(false))
}

// TODO: recreate arcexplorer theme, fonts, icons
impl Render for Application {
    fn render(&mut self, _window: &mut Window, cx: &mut Context<Self>) -> impl IntoElement {
        // TODO: match size and layout from original ArcExplorer as closely as possible
        // TODO: Add additional window for settings

        v_flex()
            .size_full()
            .on_action(cx.listener(Self::on_action_open_arc))
            .on_action(cx.listener(Self::on_action_connect_to_arc))
            .on_action(cx.listener(Self::on_action_open_preferences))
            .on_mouse_down(
                MouseButton::Navigate(NavigationDirection::Back),
                cx.listener(|this, _, window, cx| {
                    this.exit_folder(window, cx);
                }),
            )
            .on_mouse_down(
                MouseButton::Navigate(NavigationDirection::Forward),
                cx.listener(|this, _, window, cx| {
                    // TODO: store history for paths?
                }),
            )
            .child(ApplicationMenu)
            .child(
                h_resizable("main-layout")
                    .child(
                        resizable_panel().child(
                            v_flex()
                                .size_full()
                                .gap_5()
                                .p_5()
                                .child(
                                    h_flex()
                                        .w_full()
                                        .justify_end()
                                        .gap_5()
                                        .child("ARC Region")
                                        .child(
                                            div().w_40().child(
                                                Select::new(&self.region_select)
                                                    .bg(cx.theme().background),
                                            ),
                                        )
                                        .child(
                                            div().w_40().child(
                                                Input::new(&self.search_input)
                                                    .bg(cx.theme().background)
                                                    .suffix(Icon::new(IconName::Search)),
                                            ),
                                        ),
                                )
                                .child(
                                    h_flex()
                                        .w_full()
                                        .gap_5()
                                        .child(self.path_bar.clone())
                                        .child(
                                            Button::new("Exit Folder")
                                                .label("Exit Folder")
                                                .secondary()
                                                .on_click(cx.listener(|this, _, window, cx| {
                                                    this.exit_folder(window, cx);
                                                })),
                                        ),
                                )
                                .child(
                                    div()
                                        .size_full()
                                        .on_children_prepainted(cx.processor(
                                            |this, child_bounds: Vec<Bounds<Pixels>>, _, cx| {
                                                // Use a workaround to compute a responsive layout.
                                                update_table_column_sizes(this, child_bounds, cx);
                                            },
                                        ))
                                        .on_action(cx.listener(Self::on_action_table_enter_folder))
                                        .on_action(cx.listener(Self::on_action_table_exit_folder))
                                        .on_action(cx.listener(Self::on_action_table_extract_file))
                                        .on_action(
                                            cx.listener(Self::on_action_table_open_parent_folder),
                                        )
                                        .child(DataTable::new(&self.table).focus_ring(false)),
                                ),
                        ),
                    )
                    .child(
                        resizable_panel()
                            .w(px(600.0))
                            .child(self.details_panel.clone()),
                    ),
            )
            .child(
                StatusBar::new()
                    .text_base()
                    .text_color(cx.theme().foreground)
                    .left(h_flex().gap_5().when(self.arc.read(cx).is_some(), |this| {
                        this.child(format!(
                            "Path: {:?}",
                            self.arc
                                .read(cx)
                                .as_ref()
                                .map(|arc| arc.file_path.clone())
                                .unwrap_or_default()
                        ))
                        .child(format!(
                            "Version: {:?}",
                            self.arc
                                .read(cx)
                                .as_ref()
                                .map(|arc| arc_version(&arc.arc))
                                .unwrap_or_default()
                        ))
                        .child(format!(
                            "File Count: {:?}",
                            self.arc
                                .read(cx)
                                .as_ref()
                                .map(|arc| arc_file_count(&arc.arc))
                                .unwrap_or_default()
                        ))
                    })),
            )
    }
}

fn update_table_column_sizes(
    this: &mut Application,
    child_bounds: Vec<Bounds<Pixels>>,
    cx: &mut Context<Application>,
) {
    // Compute the size of the table before it is rendered.
    let table_width = child_bounds[0].size.width;
    let delegate = this.table.read(cx).delegate();

    let column_widths: Pixels = delegate.columns[1..delegate.columns.len()]
        .iter()
        .map(|col| col.width)
        .sum();
    let remaining = table_width - column_widths;

    // Datatable doesn't support responsive layouts like table.
    // Make the name column fill the remaining space.
    this.table.update(cx, |state, cx| {
        if let Some(last_col) = state.delegate_mut().columns.first_mut() {
            last_col.width = remaining;
            // TODO: this breaks column sorting
            state.refresh(cx);
        }
    })
}

// TODO: move to module
// TODO: create actions for opening folder and file
struct PreferencesWindow {
    preferences: Entity<ApplicationPreferences>,
    application_theme_select: Entity<SelectState<Vec<String>>>,
    display_format_select: Entity<SelectState<Vec<String>>>,
    extract_location_input: Entity<InputBaseState<InputMode>>,
    arc_startup_input: Entity<InputBaseState<InputMode>>,
}

impl PreferencesWindow {
    fn new(
        window: &mut Window,
        cx: &mut Context<Self>,
        preferences: Entity<ApplicationPreferences>,
    ) -> Self {
        // TODO: initialize these values from preferences.
        let application_theme_select = cx.new(|cx| {
            SelectState::new(
                vec!["Light".to_string(), "Dark".to_string()],
                Some(IndexPath::new(1)),
                window,
                cx,
            )
        });

        let display_format_select = cx.new(|cx| {
            SelectState::new(
                vec![
                    "Decimal".to_string(),
                    "Hexadecimal".to_string(),
                    "Binary".to_string(),
                ],
                Some(IndexPath::new(0)),
                window,
                cx,
            )
        });

        let extract_location_input = cx.new(|cx| InputState::new(window, cx));
        let arc_startup_input = cx.new(|cx| InputState::new(window, cx));

        Self {
            preferences: preferences.clone(),
            application_theme_select,
            display_format_select,
            extract_location_input,
            arc_startup_input,
        }
    }
}

impl Render for PreferencesWindow {
    fn render(&mut self, window: &mut Window, cx: &mut Context<Self>) -> impl IntoElement {
        // TODO: align button sizes
        h_form()
            .p_5()
            .label_width(px(200.0))
            .child(
                field()
                    .label("Application Theme")
                    .child(Select::new(&self.application_theme_select).bg(cx.theme().background)),
            )
            .child(
                field()
                    .label("File Properties Display Format")
                    .child(Select::new(&self.display_format_select).bg(cx.theme().background)),
            )
            .child(
                field().label("Extract Location").child(
                    h_flex()
                        .child(styled_input(
                            "extract-location",
                            &self.extract_location_input,
                            window,
                            cx,
                        ))
                        .child(Button::new("open-folder").label("Open Folder").secondary()),
                ),
            )
            .child(
                field().label("Open ARC on startup").child(
                    h_flex()
                        .child(styled_input(
                            "arc-startup",
                            &self.arc_startup_input,
                            window,
                            cx,
                        ))
                        .child(Button::new("open-file").label("Open File").secondary()),
                ),
            )
            // TODO: left align these fields?
            // TODO: match checkbox background to arcexplorer
            .child(
                field().child(
                    Checkbox::new("start-maximized")
                        .label("Start application maximized")
                        .checked(self.preferences.read(cx).start_maximized)
                        .on_change(cx.listener(|this, checked, _, cx| {
                            this.preferences
                                .update(cx, |state, _| state.start_maximized = *checked);
                        })),
                ),
            )
            .child(
                field().child(
                    Checkbox::new("merge-directories")
                        .label("Merge directories differing by a trailing slash")
                        .checked(self.preferences.read(cx).merge_trailing_slash)
                        .on_change(cx.listener(|this, checked, _, cx| {
                            this.preferences
                                .update(cx, |state, _| state.merge_trailing_slash = *checked);
                        })),
                ),
            )
    }
}

// TODO: move to module
pub struct ConnectToArcInfo {
    pub ip_address: SharedString,
}

struct ConnectToArcWindow {
    ip_input: Entity<InputBaseState<InputMode>>,
}

impl EventEmitter<ConnectToArcInfo> for ConnectToArcWindow {}

impl ConnectToArcWindow {
    fn new(window: &mut Window, cx: &mut Context<Self>) -> Self {
        // TODO: get/set preferences values
        // TODO: validate IP addresses
        // TODO: are both ipv4 and ipv6 supported?
        let ip_input = cx.new(|cx| InputState::new(window, cx).default_value("000.000.000.000"));
        Self { ip_input }
    }
}

impl Render for ConnectToArcWindow {
    fn render(&mut self, _window: &mut Window, cx: &mut Context<Self>) -> impl IntoElement {
        div()
            .grid()
            .grid_cols(2)
            .p_5()
            .gap_5()
            .child("IP Address")
            // TODO: better input for IP address?
            // TODO: tooltip
            .child(Input::new(&self.ip_input).bg(cx.theme().background))
            .child(
                div().col_span(2).child(
                    Checkbox::new("remember-settings")
                        .label("Remember these settings")
                        .tooltip("The IP address will be saved after clicking connect if checked."),
                ),
            )
            .child(
                Button::new("cancel")
                    .label("Cancel")
                    .secondary()
                    .on_click(|_, window, _| window.remove_window()),
            )
            .child(
                Button::new("connect")
                    .label("Connect")
                    .secondary()
                    .on_click(cx.listener(|this, _, window, cx| {
                        cx.emit(ConnectToArcInfo {
                            ip_address: this.ip_input.read(cx).value().clone(),
                        });
                        // Make sure the event is emitted before the window actually closes.
                        // TODO: Is there a better way to have the parent respond to submitting an address?
                        cx.defer_in(window, |_, window, _| {
                            window.remove_window();
                        });
                    })),
            )
    }
}

// TODO: hash update window using connect to arc as a reference.

fn initialize_themes(cx: &mut App) {
    // TODO: match theme to existing application
    // TODO: create custom functions for some controls to handle setting borders on hover
    // TODO: transparency for datatable cells?

    ThemeRegistry::global_mut(cx)
        .load_themes_from_str(include_str!("themes.json"))
        .unwrap();

    let theme = ThemeRegistry::global(cx)
        .themes()
        .get(&SharedString::from("Arc Explorer Dark"))
        .cloned()
        .unwrap();
    Theme::update(cx, |current| {
        current.apply_config(&theme);
        // The original Avalonia theme has no rounding.
        current.radius = px(0.0);
        // TODO: set this on the div instead to support titlebar
        current.font_size = px(14.0);
    });
}

fn main() {
    application().with_assets(AppAssets).run(|cx| {
        init(cx);

        cx.bind_keys([KeyBinding::new("cmd-q", Quit, None)]);
        cx.on_action(|_: &Quit, cx| {
            cx.quit();
        });

        initialize_themes(cx);

        let start = std::time::Instant::now();
        // TODO: log errors
        let preferences: ApplicationPreferences =
            std::fs::read_to_string(application_directory().join("ApplicationPreferences.json"))
                .ok()
                .and_then(|json| serde_json::from_str(&json).ok())
                .unwrap_or_default();
        println!("Load preferences: {:?}", start.elapsed());

        let options = WindowOptions {
            window_bounds: if preferences.start_maximized {
                Some(WindowBounds::Maximized(Bounds::default()))
            } else {
                None
            },
            titlebar: Some(TitlebarOptions {
                title: Some(concat!("Arc Explorer v", env!("CARGO_PKG_VERSION")).into()),
                ..Default::default()
            }),
            ..Default::default()
        };

        let (main_window, _) = open_window(options, cx, |window, cx| {
            cx.new(|cx| Application::new(window, cx, preferences))
        })
        .unwrap();

        let main_window_id = main_window.window_id();

        cx.on_window_closed(move |cx, closed_id| {
            if closed_id == main_window_id {
                // TODO: save preferences on close

                // Quit the application if the main window closes.
                // This isn't standard for MacOS but matches the original ArcExplorer's behavior.
                cx.quit();
            }
        })
        .detach();
    });
}
