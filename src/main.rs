use std::{borrow::Cow, sync::LazyLock};

use directories::ProjectDirs;
use gpui_kit::{
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
        menu::DropdownMenu,
        select::{Select, SelectItem, SelectState},
        status_bar::StatusBar,
        table::{Column, ColumnSort, DataTable, TableDelegate, TableEvent, TableState},
    },
    prelude::FluentBuilder,
    *,
};
use rfd::AsyncFileDialog;
use smash_arc::{ArcFile, Hash40, SearchCache};

use crate::{
    arc::{FileNodeInfo, arc_file_count, arc_version, load_files, search_arc},
    preferences::ApplicationPreferences,
};

mod arc;
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
                Column::new("name", "Name").sortable(),
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
            "compressed_size" => row.compressed_size().to_string().into_any_element(),
            "decompressed_size" => row.decompressed_size().to_string().into_any_element(),
            _ => div().into_any_element(),
        }
    }

    // TODO: test sorting for all sortable columns
    fn perform_sort(
        &mut self,
        col_ix: usize,
        sort: ColumnSort,
        _window: &mut Window,
        _cx: &mut Context<TableState<Self>>,
    ) {
        // TODO: sorting function
        // TODO: avoid clone
        // TODO: sort sizes by integer values and not string comparison.
        // TODO store default order for disabled sorting?
        match self.columns[col_ix].key.as_ref() {
            "name" => match sort {
                // TODO: should the default order by alphabetical to match ArcExplorer?
                ColumnSort::Default => (),
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
    }

    fn context_menu(
        &mut self,
        _row_ix: usize,
        menu: component::menu::PopupMenu,
        _window: &mut Window,
        _cx: &mut Context<TableState<Self>>,
    ) -> component::menu::PopupMenu {
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
        TableOpenParentFolder
    ]
);

struct ArcData {
    arc: ArcFile,
    search_cache: SearchCache,
    file_path: String,
}

impl ArcData {
    fn from_path<P: AsRef<std::path::Path>>(path: P) -> Self {
        // TODO: avoid unwrap.
        let path = path.as_ref();
        let arc = ArcFile::open(path).unwrap();
        let search_cache = arc.generate_search_cache();
        Self {
            arc,
            search_cache,
            file_path: path.to_string_lossy().to_string(),
        }
    }
}

struct Application {
    arc: Entity<Option<ArcData>>,

    preferences: Entity<ApplicationPreferences>,

    current_path: Entity<String>,

    details_panel: Entity<FileDetailsPanel>,

    table: Entity<TableState<ArcFilesTableDelegate>>,
    region_select: Entity<SelectState<Vec<ArcRegion>>>,
    path_input: Entity<InputBaseState<InputMode>>,
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
                .map(ArcData::from_path)
        });
        println!("Load ARC: {:?}", start.elapsed());

        let preferences = cx.new(|_cx| preferences);

        initialize_keybindings(cx);

        let files = arc
            .read(cx)
            .as_ref()
            .and_then(|arc| load_files(&arc.arc, arc::ROOT_PATH, true))
            .unwrap_or_default();

        let delegate = ArcFilesTableDelegate::new(files);
        let table = cx.new(|cx| {
            TableState::new(delegate, window, cx)
                .col_resizable(false)
                .row_selectable(true)
                .col_selectable(false)
                .cell_selectable(false)
        });
        let path_input = cx.new(|cx| InputState::new(window, cx));
        let search_input = cx.new(|cx| InputState::new(window, cx).placeholder("Search ARC"));

        let path_input_sub =
            cx.subscribe_in(&path_input, window, |this, state, event, window, cx| {
                if let InputEvent::Change = event {
                    let value = state.read(cx).value();
                    this.change_folder(cx, window, value.to_string(), true);
                }
            });

        let search_input_sub =
            cx.subscribe_in(&search_input, window, |this, state, event, window, cx| {
                if let InputEvent::Change = event {
                    let search_term = state.read(cx).value();
                    if !search_term.is_empty() {
                        this.search_files(search_term.as_str(), cx);
                    } else {
                        // No search term should restore the files prior to searching.
                        let folder_path = this.current_path.read(cx);
                        this.change_folder(cx, window, folder_path.clone(), true);
                    }
                }
            });

        let table_sub = cx.subscribe_in(&table, window, |this, table, event, window, cx| {
            match event {
                TableEvent::SelectRow(index) => {
                    let delegate = table.read(cx).delegate();
                    if let Some(file) = delegate.files.get(*index).cloned() {
                        // TODO: figure out why getting shared files is slow.
                        let start = std::time::Instant::now();
                        let shared_files = this
                            .arc
                            .read(cx)
                            .as_ref()
                            .map(|arc| file.shared_files(&arc.arc))
                            .unwrap_or_default();
                        println!("Load shared files: {:?}", start.elapsed());

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
                        let clicked_path = file.path().to_string();
                        this.change_folder(cx, window, clicked_path, true);
                    }
                }
                _ => (),
            }
        });

        // TODO: Select the specified region from preferences.
        let region_select = cx.new(|cx| {
            SelectState::new(
                vec![
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
                ],
                Some(IndexPath::new(1)),
                window,
                cx,
            )
        });

        let current_path = cx.new(|_cx| String::new());

        let details_panel = cx.new(|cx| FileDetailsPanel::new(window, cx));

        Self {
            arc,
            preferences,
            current_path,
            details_panel,
            table,
            region_select,
            path_input,
            search_input,
            _subscriptions: vec![table_sub, path_input_sub, search_input_sub],
        }
    }

    fn change_folder(
        &mut self,
        cx: &mut Context<Application>,
        window: &mut Window,
        folder_path: String,
        merge_trailing_slash: bool,
    ) {
        // TODO: normalize paths (add to arc module with tests)
        // TODO: is loading no files on invalid paths the most reasonable behavior?
        let cleaned_path = folder_path.replace("\\", "/").replace("//", "/");

        let new_files = self
            .arc
            .read(cx)
            .as_ref()
            .and_then(|arc| load_files(&arc.arc, cleaned_path.as_str(), merge_trailing_slash))
            .unwrap_or_default();

        self.path_input.update(cx, |state, cx| {
            state.set_value(folder_path.clone(), window, cx)
        });

        self.table.update(cx, |state, cx| {
            self.current_path.write(cx, folder_path);
            state.delegate_mut().files = new_files;
        });

        cx.notify();
    }

    fn search_files(&mut self, search_term: &str, cx: &mut Context<Application>) {
        // TODO: does this need to be debounced and/or made async?
        if let Some(arc) = self.arc.read(cx).as_ref() {
            let start = std::time::Instant::now();
            let new_files = search_arc(&arc.arc, &arc.search_cache, search_term);
            println!("Search: {:?}", start.elapsed());

            self.table.update(cx, |state, _cx| {
                state.delegate_mut().files = new_files;
            });

            cx.notify();
        }
    }

    fn exit_folder(&mut self, window: &mut Window, cx: &mut Context<'_, Application>) {
        let current_path = self.current_path.read(cx);
        if let Some(parent) = std::path::Path::new(current_path).parent()
            && !parent.is_empty()
        {
            let new_folder = parent.to_string_lossy().to_string();
            self.change_folder(cx, window, new_folder, true);
        } else {
            self.change_folder(cx, window, arc::ROOT_PATH.to_string(), true);
        }
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
                    let new_arc = ArcData::from_path(path);

                    view.arc.write(cx, Some(new_arc));
                    view.change_folder(cx, window, arc::ROOT_PATH.to_string(), true);
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
        _window: &mut Window,
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
        open_window(options, cx, |window, cx| {
            cx.new(|cx| ConnectToArcWindow::new(window, cx))
        })
        .unwrap();
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
            // TODO: don't enter into file paths
            let clicked_path = file.path().to_string();
            self.change_folder(cx, window, clicked_path, true);
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
                self.change_folder(cx, window, new_folder, true);
            } else {
                self.change_folder(cx, window, arc::ROOT_PATH.to_string(), true);
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
        KeyBinding::new("left", TableExitFolder, None),
    ]);
}

fn application_menu() -> impl IntoElement {
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

#[derive(Default)]
struct SharedFilesListDelegate {
    items: Vec<String>,
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
                .child(Label::new(item.clone()).text_sm().truncate())
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
    fn render(&mut self, window: &mut Window, cx: &mut Context<Self>) -> impl IntoElement {
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
                        .child(if *is_compressed {
                            "True".to_string()
                        } else {
                            "False".to_string()
                        })
                        .child("Compressed Size")
                        .child(compressed_size.clone())
                        .child("Decompressed Size")
                        .child(decompressed_size.clone())
                        .when(*is_regional, |this| {
                            this.child(
                                h_flex()
                                    .gap(px(10.0))
                                    .child(Icon::new(IconName::Globe))
                                    .child("Regional".to_string()),
                            )
                            .child("True".to_string())
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
    fn render(&mut self, window: &mut Window, cx: &mut Context<Self>) -> impl IntoElement {
        // TODO: match size and layout from original ArcExplorer as closely as possible
        // TODO: Add additional window for settings

        v_flex()
            .size_full()
            .on_action(cx.listener(Self::on_action_open_arc))
            .on_action(cx.listener(Self::on_action_connect_to_arc))
            .on_action(cx.listener(Self::on_action_open_preferences))
            .child(application_menu())
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
                                        // .child(
                                        //     // TODO Implement this manually using file explorer as a reference
                                        //     // TODO: create a component that stores state for path components?
                                        //     h_flex()
                                        //         .child(Button::new("a").label("A"))
                                        //         .child(
                                        //             // TODO: animate rotation and show popup menu with child folders
                                        //             // TODO: convert to path input on click and bread crumbs on enter
                                        //             Button::new("a2")
                                        //                 .ghost()
                                        //                 .icon(IconName::ChevronRight)
                                        //                 .dropdown_menu(|menu, window, cx| {
                                        //                     // TODO: populate with all the child folders
                                        //                     // TODO: action that stores the path or hash to open?
                                        //                     menu.menu(
                                        //                         "B",
                                        //                         Box::new(TableEnterFolder),
                                        //                     )
                                        //                 }),
                                        //         )
                                        //         .child(Button::new("b").label("B"))
                                        //         .child(
                                        //             Button::new("b2")
                                        //                 .ghost()
                                        //                 .icon(IconName::ChevronRight),
                                        //         )
                                        //         .child(Button::new("c").label("C")),
                                        // )
                                        .child(styled_input(
                                            "path-input",
                                            &self.path_input,
                                            window,
                                            cx,
                                        ))
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
                StatusBar::new().text_color(cx.theme().foreground).left(
                    h_flex()
                        .gap_5()
                        .child(format!(
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
                        )),
                ),
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
struct ConnectToArcWindow {
    ip_input: Entity<InputBaseState<InputMode>>,
}

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
                    .on_click(cx.listener(|_this, _, _window, _cx| {
                        // TODO: connect to arc over network asynchronously
                    })),
            )
    }
}

fn initialize_themes(cx: &mut App) {
    // TODO: match theme to existing application
    ThemeRegistry::watch_dir("src".into(), cx, move |cx| {
        let theme = ThemeRegistry::global(cx)
            .themes()
            .get(&SharedString::from("Arc Explorer Dark"))
            .cloned()
            .unwrap();
        Theme::update(cx, |current| {
            current.apply_config(&theme);
            current.radius = px(0.0);
            current.focus_ring = false;
        });
    })
    .unwrap();

    // TODO: create custom functions for some controls to handle setting borders on hover
    // TODO: transparency for datatable cells?

    // TODO: use this to load the theme from memory
    // ThemeRegistry::global_mut(cx)
    //     .load_themes_from_str(include_str!("themes.json"))
    //     .unwrap();

    // let theme = ThemeRegistry::global(cx)
    //     .themes()
    //     .get(&SharedString::from("Arc Explorer Dark"))
    //     .cloned()
    //     .unwrap();
    // Theme::update(cx, |current| current.apply_config(&theme));
}

fn main() {
    application().with_assets(AppAssets).run(|cx| {
        init(cx);

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
        // TODO: save preferences on close
        open_window(options, cx, |window, cx| {
            cx.new(|cx| Application::new(window, cx, preferences))
        })
        .unwrap();
    });
}
