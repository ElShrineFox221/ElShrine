using System.Windows;

namespace ElShrine.Wpf.Controls.Transitions
{
    public interface ITransition
    {
        bool TryDoTransition(DependencyObject tar, DependencyProperty tarDpProp, object? tarValue, bool isIn);
    }
}
