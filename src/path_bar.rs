use gpui_kit::{
    Entity, FocusHandle, SharedString, Subscription, Window,
    assets::IconName,
    base::h_flex,
    component::{
        ActiveTheme,
        button::{Button, ButtonVariants},
        input::{Input, InputEvent, InputState},
    },
    div,
    prelude::*,
};
use itertools::Itertools;

pub struct PathBar {
    pub current_path: SharedString,
    is_editing: bool,
    input: Entity<InputState>,
    div_focus_handle: FocusHandle,
    _subscriptions: Vec<Subscription>,
}

impl PathBar {
    pub fn new(path: SharedString, window: &mut Window, cx: &mut Context<Self>) -> Self {
        let input = cx.new(|cx| InputState::new(window, cx).default_value(path.clone()));

        let sub = cx.subscribe_in(
            &input,
            window,
            |this, state, event, _window, cx| match event {
                InputEvent::PressEnter { .. } => {
                    this.current_path = state.read(cx).value();
                    this.is_editing = false;
                    cx.notify();
                }
                InputEvent::Blur => {
                    this.is_editing = false;
                    cx.notify();
                }
                _ => {}
            },
        );

        let div_focus_handle = cx.focus_handle().tab_stop(true);

        Self {
            current_path: path,
            is_editing: false,
            input,
            div_focus_handle,
            _subscriptions: vec![sub],
        }
    }

    fn enter_edit_mode(&mut self, window: &mut Window, cx: &mut Context<Self>) {
        self.is_editing = true;
        let path = self.current_path.clone();
        self.input.update(cx, |state, cx| {
            state.set_value(path, window, cx);
            state.focus(window, cx);
        });
        cx.notify();
    }
}

impl Render for PathBar {
    fn render(&mut self, _window: &mut Window, cx: &mut Context<Self>) -> impl IntoElement {
        if self.is_editing {
            div()
                .w_full()
                .child(Input::new(&self.input).bg(cx.theme().background))
                .into_any_element()
        } else {
            let path = std::path::Path::new(self.current_path.as_str());
            let segments = path.components().with_position();

            let mut children = h_flex();
            for (i, (pos, s)) in segments.enumerate() {
                // TODO: avoid clone?
                let name = s.as_os_str().to_string_lossy().to_string();
                children = children.child(
                    Button::new(name.clone())
                        .ghost()
                        .label(name.clone())
                        .on_click(cx.listener(move |this, _, _window, cx| {
                            // Calculate the path up to and including the current component.
                            let path = std::path::Path::new(this.current_path.as_str());
                            let new_path: std::path::PathBuf =
                                path.components().take(i + 1).collect();
                            this.current_path = new_path.to_string_lossy().into();
                            cx.stop_propagation(); // don't enter edit mode
                            cx.notify();
                        })),
                );
                if !pos.is_last() {
                    children = children.child(
                        Button::new(format!("{name}.right"))
                            .ghost()
                            .icon(IconName::ChevronRight)
                            .on_click(cx.listener(|this, _, _window, cx| {
                                // TODO: dropdown_menu with entries to navigate to child folders
                                cx.stop_propagation(); // don't enter edit mode      
                                cx.notify();
                            })),
                    );
                }
            }

            // Create a border to show that the empty area is interactable.
            // TODO: clicking buttons should not enter edit mode.
            div()
                .id("path-breadcrumb-bar")
                .track_focus(&self.div_focus_handle)
                .w_full()
                .border_1()
                .border_color(cx.theme().input)
                .focus(|style| style.border_color(cx.theme().ring))
                .hover(|style| style.border_color(cx.theme().ring))
                .cursor_text()
                .on_click(cx.listener(|this, _event, window, cx| {
                    this.enter_edit_mode(window, cx);
                }))
                .child(children)
                .into_any_element()
        }
    }
}
