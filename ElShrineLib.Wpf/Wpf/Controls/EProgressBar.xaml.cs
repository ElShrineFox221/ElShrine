using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.Controls.Extensions;
using ElShrine.Wpf.UITheme;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;


namespace ElShrine.Wpf.Controls;

public enum EProgressBarStyle
{
    Horizontal = 0,
    Circular = 1
}
[GenerateDPCli]
public partial class EProgressBar : ContentControl, IThemeControlBase
{
    static EProgressBar() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EProgressBar), new FrameworkPropertyMetadata(typeof(EProgressBar)));
    public EProgressBar() => WpfModuleAccessor.UITheme.RegisterCoerceThemeDPs(this);
    public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => TransHelper.CoerceValue(this);
    public void LocalThemePropertyChanged(DependencyPropertyChangedEventArgs e) => WpfModuleAccessor.StateListener.RedoSetterTransitions(this);

    #region Dependency Properties - Additional
    public bool IsIndeterminate
    {
        get => (bool)GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }
    public static readonly DependencyProperty IsIndeterminateProperty = DependencyProperty.Register(nameof(IsIndeterminate), typeof(bool), typeof(EProgressBar), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsIndeterminateChanged));

    public double IndeterminateProgressRate
    {
        get => (double)GetValue(IndeterminateProgressRateProperty);
        set => SetValue(IndeterminateProgressRateProperty, value);
    }
    public static readonly DependencyProperty IndeterminateProgressRateProperty = DependencyProperty.Register(nameof(IndeterminateProgressRate), typeof(double), typeof(EProgressBar), new PropertyMetadata(0.3d, OnIndeterminateProgressRateChanged));

    public double TransXNorm
    {
        get => (double)GetValue(TransXNormProperty);
        set => SetValue(TransXNormProperty, value);
    }
    public static readonly DependencyProperty TransXNormProperty = DependencyProperty.Register(nameof(TransXNorm), typeof(double), typeof(EProgressBar), new PropertyMetadata(0d, OnTransXNormChanged));

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, Math.Clamp(value, 0, 1));
    }
    public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(nameof(Progress), typeof(double), typeof(EProgressBar), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender, OnProgressChanged, CoerceProgressValue));

    public EProgressBarStyle ProgressBarStyle
    {
        get => (EProgressBarStyle)GetValue(ProgressBarStyleProperty);
        set => SetValue(ProgressBarStyleProperty, value);
    }
    public static readonly DependencyProperty ProgressBarStyleProperty = DependencyProperty.Register(nameof(ProgressBarStyle), typeof(EProgressBarStyle), typeof(EProgressBar), new FrameworkPropertyMetadata(EProgressBarStyle.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public double RingThickness
    {
        get => (double)GetValue(RingThicknessProperty);
        set => SetValue(RingThicknessProperty, value);
    }
    public static readonly DependencyProperty RingThicknessProperty = DependencyProperty.Register(nameof(RingThickness), typeof(double), typeof(EProgressBar), new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public double AnimatedProgress
    {
        get => (double)GetValue(AnimatedProgressProperty);
        private set => SetValue(AnimatedProgressProperty, value);
    }
    public static readonly DependencyProperty AnimatedProgressProperty = DependencyProperty.Register(nameof(AnimatedProgress), typeof(double), typeof(EProgressBar), new FrameworkPropertyMetadata(0.0, OnAnimatedProgressChanged));
    
    #endregion

    #region Property Change Handlers
    private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EProgressBar control && e.NewValue is double newValue)
        {
            control.AnimateProgress(newValue);
        }
    }
    private static object CoerceProgressValue(DependencyObject d, object baseValue)
    {
        double value = (double)baseValue;
        if (value < 0) return 0;
        if (value > 1) return 1;
        return value;
    }
    private static void OnIsIndeterminateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EProgressBar control)
        {
            if ((bool)e.NewValue)
                control.StartIndeterminateAnimation();
            else
                control.StopIndeterminateAnimation();
            control.UpdateProgress();
        }
    }
    private static void OnTransXNormChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if(d is EProgressBar control && control.IsIndeterminate)
        {
            control.UpdateProgress();
        }
    }
    private static void OnIndeterminateProgressRateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if(d is EProgressBar control)
        {
            control.UpdateProgress();
        }
    }
    private static void OnAnimatedProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if(d is EProgressBar control && !control.IsIndeterminate)
        {
            control.UpdateProgress();
        }
    }
    #endregion

    private Border? PART_ProgressIndicator;
    private Border? PART_Background;
    private TranslateTransform? _trans;
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        PART_ProgressIndicator = GetTemplateChild(nameof(PART_ProgressIndicator)) as Border;
        PART_Background = GetTemplateChild(nameof(PART_Background)) as Border;
        //
        PART_Background?.SizeChanged+= (s, e) =>
        {
            UpdateProgress();
        };
        if (PART_Background is not null)
        {
            _trans = new TranslateTransform();
            PART_Background.RenderTransform = _trans;
        }
        

        //
        UpdateProgress();
    }

    #region Animation Logic

    private void AnimateProgress(double targetProgress)
    {
        var duration = new Duration(TimeSpan.FromSeconds(AnimaDurationIn > 0 ? AnimaDurationIn : 0.4));
        var easing = AnimaEaseFunc ?? new SineEase { EasingMode = EasingMode.EaseOut };

        var animation = new DoubleAnimation
        {
            To = targetProgress,
            Duration = duration,
            EasingFunction = easing
        };

        BeginAnimation(AnimatedProgressProperty, animation);
    }

    private Storyboard? _indeterminateStoryboard;
    private void StartIndeterminateAnimation()
    {
        StopIndeterminateAnimation();
        var animation = new DoubleAnimation
        {
            From = 0.0,
            To = 1.0,
            Duration = new Duration(TimeSpan.FromSeconds(Math.Max(3 * (AnimaDurationIn + AnimaDurationOut), 1))),
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = AnimaEaseFunc
        };

        _indeterminateStoryboard = new Storyboard();
        _indeterminateStoryboard.Children.Add(animation);
        Storyboard.SetTarget(animation, this);
        // 绑定到 TransXNorm 属性
        Storyboard.SetTargetProperty(animation, new PropertyPath(TransXNormProperty));
        _indeterminateStoryboard.Begin();
    }
    private void StopIndeterminateAnimation()
    {
        if (_indeterminateStoryboard != null)
        {
            _indeterminateStoryboard.Stop();
            ClearValue(TransXNormProperty);
            _indeterminateStoryboard = null;
        }
    }
    #endregion

    private void UpdateProgress()
    {
        if (IsIndeterminate)
            UpdateProgressInterminate();
        else
        {
            if (PART_ProgressIndicator is null || PART_Background is null)
                return;
            var totalWidth = PART_Background.ActualWidth;
            PART_ProgressIndicator.Width = totalWidth * AnimatedProgress;
            PART_ProgressIndicator.Opacity = 1;
            _trans?.X = 0;
        }
    }
    private void UpdateProgressInterminate()
    {
        if (PART_ProgressIndicator is null || PART_Background is null) 
            return;
        var totalWidth = PART_Background.ActualWidth;
        var transX = TransXNorm;
        var barWidthRatio = IndeterminateProgressRate;
        if (totalWidth <= 0 || barWidthRatio <= 0 || barWidthRatio >= 1)
        {
            PART_ProgressIndicator.Visibility = Visibility.Hidden;
            return;
        }
        PART_ProgressIndicator.Visibility = Visibility.Visible;
        // do calculation
        //
        double normalizedDistance = Math.Abs(transX - 0.5);
        double scaledDistance = 2.0 * normalizedDistance;
        double opacityInverse = Math.Pow(scaledDistance, 4.0);
        double opacity = 1.0 - opacityInverse;
        PART_ProgressIndicator.Opacity = opacity;
        //
        double tx = (transX * (1.0 + barWidthRatio) - barWidthRatio) * totalWidth;

        double right_clamp = Math.Min(tx + barWidthRatio * totalWidth, totalWidth);

        double x_geom = Math.Max(tx, 0);
        double w_geom = Math.Max(0, right_clamp - x_geom);

        _trans?.X = x_geom;
        PART_ProgressIndicator.Width = w_geom;
    }
}