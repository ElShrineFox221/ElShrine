using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.Controls.Extensions;
using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls;

namespace ElShrine.Wpf.Controls;

[GenerateDPCli]
public partial class EScrollViewer : ScrollViewer, IThemeControlBase, IScrollBarControlBase, IScrollBarControllerBase
{
    #region Implements
    static EScrollViewer() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EScrollViewer), new FrameworkPropertyMetadata(typeof(EScrollViewer)));
    public EScrollViewer() => WpfModuleAccessor.UITheme.RegisterCoerceThemeDPs(this);
    public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => TransHelper.CoerceValue(this);
    public void LocalThemePropertyChanged(DependencyPropertyChangedEventArgs e) => WpfModuleAccessor.StateListener.RedoSetterTransitions(this);
    #endregion

    protected override void OnScrollChanged(ScrollChangedEventArgs e)
    {
        base.OnScrollChanged(e);
        if (e.ExtentHeightChange != 0)
            ProcessAutoScroll(e.VerticalOffset, e.ViewportHeight, e.ExtentHeight - e.ExtentHeightChange, VerticalScrollToEnd, true);
        if (e.ExtentWidthChange != 0)
            ProcessAutoScroll(e.HorizontalOffset, e.ViewportWidth, e.ExtentWidth - e.ExtentWidthChange, HorizontalScrollToEnd, false);
    }
    private void ProcessAutoScroll(double currentOffset, double viewportSize, double oldExtent, ScrollToEndMode mode, bool isVertical)
    {
        if ((mode & ScrollToEndMode.EnabledFlag) == 0) return;
        bool shouldScroll = false;
        bool isAlways = (mode & ScrollToEndMode.AutoFlag) == 0;
        if (isAlways) shouldScroll = true;
        else
        {
            if ((mode & ScrollToEndMode.ToRightOrBottom) != 0)
            {
                if (currentOffset + viewportSize >= oldExtent - 1.0) shouldScroll = true;
            }
            else if ((mode & ScrollToEndMode.ToLeftOrTop) != 0)
            {
                if (currentOffset <= 1.0) shouldScroll = true;
            }
        }

        if (shouldScroll)
        {
            if (isVertical)
            {
                if ((mode & ScrollToEndMode.ToRightOrBottom) != 0) ScrollToBottom();
                else ScrollToTop();
            }
            else
            {
                if ((mode & ScrollToEndMode.ToRightOrBottom) != 0) ScrollToRightEnd();
                else ScrollToLeftEnd();
            }
        }
    }
}
