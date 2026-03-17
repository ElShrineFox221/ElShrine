using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ElShrine.Modules.StateListener;

internal static class CommonStateCalculator
{
    public static string CalculateMouseState(UIElement ele)
    {
        var @enum = MouseStateGroup.MouseOut;
        if (ele.IsMouseCaptured || (Mouse.LeftButton == MouseButtonState.Pressed && ele.IsMouseOver))
            @enum = MouseStateGroup.MouseDown;
        else if (ele.IsMouseOver) @enum = MouseStateGroup.MouseIn;
        return @enum.ToString();
    }
    public static string CalculateSelectionState(UIElement ele)
    {
        var selected = false;
        if (ele is ListBoxItem lbi) selected = lbi.IsSelected;
        else if (ele is TabItem ti) selected = ti.IsSelected;
        else if (ele is ToggleButton tb) selected = tb.IsChecked ?? false;
        var @enum = selected ? SelectionStateGroup.Selected : SelectionStateGroup.Unselected;
        return @enum.ToString();
    }
    public static string CalculateFocusState(UIElement ele)
    {
        var @enum = ele.IsFocused ? FocusStateGroup.Focused : FocusStateGroup.UnFocused;
        return @enum.ToString();
    }
}