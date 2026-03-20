using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.Controls.Extensions;
using ElShrine.Wpf.UITheme;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static ElShrine.WindowsHandler;

namespace ElShrine.Wpf.Controls;

public partial class EWindow : Window, IThemeControlBase
{
    #region DPs

    #region Header
    [TypeConverter(typeof(LengthConverter))]
    public double TitleBarHeight
    {
        get => (double)GetValue(TitleBarHeightProperty);
        set => SetValue(TitleBarHeightProperty, value);
    }
    public object HeaderContent
    {
        get => GetValue(HeaderContentProperty);
        set => SetValue(HeaderContentProperty, value);
    }
    public DataTemplate HeaderTemplate
    {
        get => (DataTemplate)GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }
    public static readonly DependencyProperty HeaderContentProperty = DependencyProperty.Register(nameof(HeaderContent), typeof(object), typeof(EWindow), new PropertyMetadata(null));
    public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register(nameof(HeaderTemplate), typeof(DataTemplate), typeof(EWindow), new PropertyMetadata(null));
    public static readonly DependencyProperty TitleBarHeightProperty = DependencyProperty.Register(
        nameof(TitleBarHeight), typeof(double), typeof(EWindow), new PropertyMetadata((double)24));

    #region HeaderBtns
    public bool IsCloseButtonVisible
    {
        get => (bool)GetValue(IsCloseButtonVisibleProperty);
        set => SetValue(IsCloseButtonVisibleProperty, value);
    }
    public static readonly DependencyProperty IsCloseButtonVisibleProperty =
        DependencyProperty.Register(nameof(IsCloseButtonVisible), typeof(bool), typeof(EWindow), new PropertyMetadata(true));

    public bool IsMinimizeButtonVisible
    {
        get => (bool)GetValue(IsMinimizeButtonVisibleProperty);
        set => SetValue(IsMinimizeButtonVisibleProperty, value);
    }
    public static readonly DependencyProperty IsMinimizeButtonVisibleProperty = DependencyProperty.Register(nameof(IsMinimizeButtonVisible), typeof(bool), typeof(EWindow), new PropertyMetadata(true));

    public bool IsMaximizeRestoreButtonVisible
    {
        get => (bool)GetValue(IsMaximizeRestoreButtonVisibleProperty);
        set => SetValue(IsMaximizeRestoreButtonVisibleProperty, value);
    }
    public static readonly DependencyProperty IsMaximizeRestoreButtonVisibleProperty = DependencyProperty.Register(nameof(IsMaximizeRestoreButtonVisible), typeof(bool), typeof(EWindow), new PropertyMetadata(true));

    public DataTemplate HeaderButtonTemplate
    {
        get => (DataTemplate)GetValue(HeaderButtonTemplateProperty);
        set => SetValue(HeaderButtonTemplateProperty, value);
    }
    public static readonly DependencyProperty HeaderButtonTemplateProperty = DependencyProperty.Register(nameof(HeaderButtonTemplate), typeof(DataTemplate), typeof(EWindow), new PropertyMetadata(null));


    public ICommand CloseCommand
    {
        get => (ICommand)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }
    public ICommand MinimizeCommand
    {
        get => (ICommand)GetValue(MinimizeCommandProperty);
        set => SetValue(MinimizeCommandProperty, value);
    }
    public ICommand MaximizeRestoreCommand
    {
        get => (ICommand)GetValue(MaximizeRestoreCommandProperty);
        set => SetValue(MaximizeRestoreCommandProperty, value);
    }

    public static readonly DependencyProperty CloseCommandProperty = DependencyProperty.Register(nameof(CloseCommand), typeof(ICommand), typeof(EWindow), new PropertyMetadata(new VMCommand(p => CloseWindow((EWindow)p!)))); 
    public static readonly DependencyProperty MinimizeCommandProperty = DependencyProperty.Register(nameof(MinimizeCommand), typeof(ICommand), typeof(EWindow), new PropertyMetadata(new VMCommand(p => MinimizeWindow((EWindow)p!)))); 
    public static readonly DependencyProperty MaximizeRestoreCommandProperty = DependencyProperty.Register(nameof(MaximizeRestoreCommand), typeof(ICommand), typeof(EWindow), new PropertyMetadata(new VMCommand(p => MaximizeOrRestoreWindow((EWindow)p!))));
    #endregion

    #endregion

    #region Header Extra Info

    #region MaximizeOrRestore Icon DP
    private const string MaximizeIconText = "\xE922";
    private const string RestoreIconText = "\xE923";

    public string MaxOrRestoreIcon
    {
        get => (string)GetValue(MaxOrRestoreIconProperty);
        set => SetValue(MaxOrRestoreIconProperty, value);
    }
    public static readonly DependencyProperty MaxOrRestoreIconProperty = DependencyProperty.Register(nameof(MaxOrRestoreIcon), typeof(string), typeof(EWindow), new PropertyMetadata(MaximizeIconText));
    #endregion

    #endregion

    #region Border
    public bool IsSelectionWindowEnabled
    {
        get => (bool)GetValue(IsSelectionWindowEnabledProperty);
        set => SetValue(IsSelectionWindowEnabledProperty, value);
    }

    public static readonly DependencyProperty IsSelectionWindowEnabledProperty = DependencyProperty.Register(nameof(IsSelectionWindowEnabled), typeof(bool), typeof(EWindow), new PropertyMetadata(true));
    #endregion

    public readonly static DependencyProperty AllowDragResizeProperty = DependencyProperty.Register(nameof(AllowDragResize), typeof(bool), typeof(EWindow), new PropertyMetadata(true));
    public bool AllowDragResize
    {
        get => (bool)GetValue(AllowDragResizeProperty);
        set => SetValue(AllowDragResizeProperty, value);
    }
    #endregion

    private Border? MainBorder;
    private Grid? TitleBarGrid;
    private Grid? MainContainerGrid;
    static EWindow() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EWindow), new FrameworkPropertyMetadata(typeof(EWindow)));
    public EWindow() : base()
    {
        WpfModuleAccessor.UITheme.RegisterCoerceThemeDPs(this);
        StateChanged += EWindow_StateChanged;
        MouseEnter += EWindow_MouseEnter;
        MouseLeave += EWindow_MouseLeave;
    }
    public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => TransHelper.CoerceValue(this);
    public void LocalThemePropertyChanged(DependencyPropertyChangedEventArgs e) { }

    #region override
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild(nameof(MainBorder)) is Border mainBorder) MainBorder = mainBorder;
        if (GetTemplateChild(nameof(TitleBarGrid)) is Grid titleBarGrid)
        {
            TitleBarGrid = titleBarGrid;
            TitleBarGrid.MouseLeftButtonDown += TitleBarGrid_MouseLeftButtonDown;
        }
        if(GetTemplateChild(nameof(MainContainerGrid)) is Grid mainContainerGrid)
        {
            MainContainerGrid = mainContainerGrid;
        }
    }
    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        UpdateMaxRestoreIcon();
    }
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        HwndSource? source = PresentationSource.FromVisual(this) as HwndSource;
        source?.AddHook(WindowProc);
    }
    #endregion

    #region delegates
    private void EWindow_StateChanged(object? sender, EventArgs e)
    {
        UpdateMaxRestoreIcon();
        if(WindowState == WindowState.Normal)
        {
            MainBorder?.Opacity = 0;
            FadeIn(this, 0.15, null, false);
        }
    }
    private void EWindow_MouseEnter(object sender, MouseEventArgs e)
    {
        if (IsSelectionWindowEnabled) AnimateBorderBrush(PrimaryBrush, SecondaryBrush);
    }
    private void EWindow_MouseLeave(object sender, MouseEventArgs e)
    {
        if (IsSelectionWindowEnabled) AnimateBorderBrush(SecondaryBrush, PrimaryBrush);
    }
    #endregion

    #region Fade Animation
    public static void MinimizeWindow(EWindow window)
    {
        FadeOut(window, 0.0, 0.3, (e) =>
        {
            window.WindowState = WindowState.Minimized;
            window.MainBorder?.Opacity = 1;
        }, false);
    }
    public static void MaximizeOrRestoreWindow(EWindow window)
    {
        const double tempOpacity = 0.1, duraSeconds = 0.15;
        FadeOut(window, tempOpacity, duraSeconds, (e) =>
        {
            var isMax = window.WindowState == WindowState.Maximized;
            window.WindowState = isMax ? WindowState.Normal : WindowState.Maximized;
            if (!isMax) FadeIn(window, duraSeconds, null, false);
            window.UpdateMaxRestoreIcon();
            //FadeIn(window, duraSeconds, null, false);
        }, false);
    }
    public static void CloseWindow(EWindow window)
    {
        FadeOut(window, 0, 0.3, (e) =>
        {
            window.Close();
        }, false);
    }

    private static void FadeIn(EWindow window, double durationSeconds, Action<EventArgs>? completedAction, bool isFill)
    {
        var mainBorder = window.MainBorder;
        if (mainBorder is null)
        {
            completedAction?.Invoke(EventArgs.Empty);
            return;
        }
        var animation = new DoubleAnimation(mainBorder.Opacity, 1, TimeSpan.FromSeconds(durationSeconds))
        {
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
            FillBehavior = isFill ? FillBehavior.HoldEnd : FillBehavior.Stop
        };
        animation.Completed += (s, e) =>
        {
            if (!isFill) mainBorder.Opacity = 1;
            completedAction?.Invoke(e);
        };
        mainBorder.BeginAnimation(OpacityProperty, animation);
    }
    private static void FadeOut(EWindow window, double targetOpacity, double durationSeconds, Action<EventArgs>? completedAction, bool isFill)
    {
        var mainBorder = window.MainBorder;
        if (mainBorder is null)
        {
            completedAction?.Invoke(EventArgs.Empty);
            return;
        }
        var animation = new DoubleAnimation(mainBorder.Opacity, targetOpacity, TimeSpan.FromSeconds(durationSeconds))
        {
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
            FillBehavior = isFill ? FillBehavior.HoldEnd : FillBehavior.Stop
        };
        animation.Completed += (s, e) =>
        {
            if (!isFill) mainBorder.Opacity = targetOpacity;
            completedAction?.Invoke(e);
        };
        mainBorder.BeginAnimation(OpacityProperty, animation);
    }
    #endregion

    #region StateChanged Icon
    private void UpdateMaxRestoreIcon()
    {
        if (WindowState == WindowState.Maximized) MaxOrRestoreIcon = RestoreIconText;
        else MaxOrRestoreIcon = MaximizeIconText;
    }
    
    #endregion

    #region Drag
    private void TitleBarGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1 && e.LeftButton == MouseButtonState.Pressed) DragMove();
        else if (e.ClickCount == 2) MaximizeOrRestoreWindow(this);
    }
    #endregion

    #region Resize
    
    public int DragResizeEdgeWidth { get; set; } = 6;
    private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        var result = IntPtr.Zero;
        switch (msg)
        {
            case WM_NCHITTEST:
                result = HandleHitTest(lParam, ref handled);
                break;
            case WM_GETMINMAXINFO:
                handled = true;
                result = AdjustMaximizedWindow(lParam);
                break;
        }
        return result;
    }
    private nint HandleHitTest(IntPtr lParam, ref bool handled)
    {
        //Get relative position
        var screenPos = new Point((short)(lParam & 0xFFFF), (short)(lParam >> 16));
        var clientPos = PointFromScreen(screenPos);
        //Handle header hit
        var hitResult = (IsInsideTitleBar(clientPos) && (handled = true)) ? HTCLIENT : IntPtr.Zero;
        //Handle maximized or minmized window
        if (WindowState != WindowState.Normal) return hitResult;
        //Handle resize
        if (AllowDragResize)
        {
            var _hitResult = GetResizeHitZone(clientPos);
            if (_hitResult != NONE && (handled = true)) hitResult = _hitResult;
        }
        return hitResult;
    }
    private bool IsInsideTitleBar(Point clientPoint)
    {
        double titleHeight = TitleBarGrid?.ActualHeight ?? 0;
        return clientPoint.Y >= 0 && clientPoint.Y < titleHeight;
    }
    private int GetResizeHitZone(Point pt)
    {
        double w = ActualWidth, h = ActualHeight, edge = DragResizeEdgeWidth;
        double corner = edge * 2;

        if (pt.X <= corner && pt.Y <= corner) return HTTOPLEFT;
        if (pt.X >= w - corner && pt.Y <= corner) return HTTOPRIGHT; 
        if (pt.X <= corner && pt.Y >= h - corner) return HTBOTTOMLEFT; 
        if (pt.X >= w - corner && pt.Y >= h - corner) return HTBOTTOMRIGHT; 

        if (pt.X <= edge) return HTLEFT;
        if (pt.X >= w - edge) return HTRIGHT;
        if (pt.Y <= edge) return HTTOP;
        if (pt.Y >= h - edge) return HTBOTTOM;

        return NONE;
    }
    #endregion

    #region BorderColor Animation
    private void AnimateBorderBrush(Brush fromBrush, Brush toBrush)
    {
        if (MainBorder is null) return;
        if (fromBrush is not SolidColorBrush fromSolidBrush || toBrush is not SolidColorBrush toSolidBrush)
        {
            MainBorder.BorderBrush = toBrush;
            return;
        }
        var animation = new ColorAnimation
        {
            From = fromSolidBrush.Color,
            To = toSolidBrush.Color,
            Duration = TimeSpan.FromSeconds(0.3),
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
            FillBehavior = FillBehavior.HoldEnd
        };
        MainBorder.BorderBrush ??= new SolidColorBrush(fromSolidBrush.Color);
        MainBorder.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, null);
        if (MainBorder.BorderBrush != fromBrush)
        {
            if (MainBorder.BorderBrush is not SolidColorBrush currentBrush || currentBrush.Color != fromSolidBrush.Color) MainBorder.BorderBrush = fromSolidBrush.Clone();
        }
        if (MainBorder.BorderBrush is SolidColorBrush currentAnimateBrush) currentAnimateBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
    }
    #endregion

    #region Maximize Window
    private IntPtr AdjustMaximizedWindow(IntPtr lParam)
    {
        var mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO))!;
        if(MainBorder is null || MainContainerGrid is null) return IntPtr.Zero;
        var relaSize = MainContainerGrid.GetPositionRelativeTo(this);
        var offsetRelaSize = new POINT { x = (int)relaSize.X, y = (int)relaSize.Y };
        var offsetSize = new POINT { x = int.Abs(mmi.ptMaxPosition.x), y = int.Abs(mmi.ptMaxPosition.y) };
        mmi = new()
        {
            ptMaxPosition = new() { x = -offsetRelaSize.x, y = -offsetRelaSize.y },
            ptMaxSize = new() { x = mmi.ptMaxSize.x - 2 * offsetSize.x + 2 * offsetRelaSize.x, y = mmi.ptMaxSize.y - 2 * offsetSize.y + 2 * offsetRelaSize.y },
            ptMinTrackSize = new() { x = ToTrackSize(MinWidth, false), y = ToTrackSize(MinHeight, false) },
            ptMaxTrackSize = new() { x = ToTrackSize(MaxWidth, true), y = ToTrackSize(MaxHeight, true) },
        };

        Marshal.StructureToPtr(mmi, lParam, true);
        return IntPtr.Zero;
    }
    private static int ToTrackSize(double d, bool isMaxTrack)
    {
        int r;
        if (double.IsNaN(d)) r = isMaxTrack ? int.MaxValue : 0;
        else r = (int)(isMaxTrack ? Math.Min(d, int.MaxValue) : Math.Max(d, 0));
        return r;
    }
    #endregion
}
