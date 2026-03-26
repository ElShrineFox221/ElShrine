using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ElShrine.Modules.StateListener;

#region StateGroupSources
[StateGroupRule<UIElement>(
    nameof(UIElement.MouseDownEvent), nameof(UIElement.MouseUpEvent),
    nameof(UIElement.PreviewMouseDownEvent), nameof(UIElement.PreviewMouseUpEvent),
    nameof(UIElement.GotMouseCaptureEvent), nameof(UIElement.LostMouseCaptureEvent),
    nameof(UIElement.MouseEnterEvent), nameof(UIElement.MouseLeaveEvent))]
public enum MouseStateGroup
{
    MouseIn,
    MouseOut,
    MouseDown,
    MouseMove,
    Dragging
}
[StateGroupRule<ListBoxItem>(nameof(ListBoxItem.SelectedEvent), nameof(ListBoxItem.UnselectedEvent))]
[StateGroupRule<ToggleButton>(nameof(ToggleButton.CheckedEvent), nameof(ToggleButton.UncheckedEvent))]
public enum SelectionStateGroup
{
    Selected,
    Unselected
}
[StateGroupRule<UIElement>(nameof(UIElement.GotFocusEvent), nameof(UIElement.LostFocusEvent))]
public enum FocusStateGroup
{
    Focused,
    UnFocused,
}
#endregion
