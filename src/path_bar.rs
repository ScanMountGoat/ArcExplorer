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
            let segments = std::path::Path::new(self.current_path.as_str())
                .components()
                .with_position();

            let mut children = h_flex();
            for (pos, s) in segments {
                // TODO: avoid clone?
                let name = s.as_os_str().to_string_lossy().to_string();
                children = children.child(
                    Button::new(name.clone())
                        .ghost()
                        .label(name.clone())
                        .on_click(cx.listener(|_this, _, _window, cx| {
                            // TODO: enter folder by changing the path and calling cx.notify()
                            // TODO: should the main app just observe this entity's path?
                            cx.stop_propagation();
                        })),
                );
                if !pos.is_last() {
                    // TODO: dropdown_menu with child folders
                    children =
                        children.child(Button::new(name).ghost().icon(IconName::ChevronRight));
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
