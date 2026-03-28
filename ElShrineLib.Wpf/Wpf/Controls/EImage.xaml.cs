using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.Controls.Extensions;
using ElShrine.Wpf.UITheme;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace ElShrine.Wpf.Controls;

[GenerateDPCli]
public partial class EImage : Control, IThemeControlBase
{
    static EImage() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EImage), new FrameworkPropertyMetadata(typeof(EImage)));
    public EImage() => WpfModuleAccessor.UITheme.RegisterCoerceThemeDPs(this);
    public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => TransHelper.CoerceValue(this);
    public void LocalThemePropertyChanged(DependencyPropertyChangedEventArgs e) => WpfModuleAccessor.StateListener.RedoSetterTransitions(this);

    #region DPs

    #region source
    public string Url
    {
        get => (string)GetValue(UrlProperty);
        set => SetValue(UrlProperty, value);
    }
    public Task<ImageSource>? RunningTask
    {
        get => (Task<ImageSource>?)GetValue(RunningTaskProperty);
        set => SetValue(RunningTaskProperty, value);
    }
    public static readonly DependencyProperty UrlProperty = DependencyProperty.Register(nameof(Url), typeof(string), typeof(EImage), new FrameworkPropertyMetadata(null, OnUrlChanged));
    public static readonly DependencyProperty RunningTaskProperty = DependencyProperty.Register(nameof(RunningTask), typeof(Task<ImageSource>), typeof(EImage), new FrameworkPropertyMetadata(null, OnRunningTaskChanged));
    #endregion

    #region status
    public ImageSource? LoadedImageSource
    {
        get => (ImageSource?)GetValue(LoadedImageSourceProperty);
        private set => SetValue(LoadedImageSourceProperty, value);
    }
    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        private set => SetValue(IsLoadingProperty, value);
    }
    public string ErrorMsg
    {
        get => (string)GetValue(ErrorMsgProperty);
        private set => SetValue(ErrorMsgProperty, value);
    }
    public Stretch Stretch
    {
        get => (Stretch)GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }
    public double FallbackAspectRatio
    {
        get => (double)GetValue(FallbackAspectRatioProperty);
        set => SetValue(FallbackAspectRatioProperty, value);
    }
    

    public static readonly DependencyProperty LoadedImageSourceProperty = DependencyProperty.Register(nameof(LoadedImageSource), typeof(ImageSource), typeof(EImage), new PropertyMetadata(null));
    public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(EImage), new PropertyMetadata(false, OnIsLoadingChanged));
    public static readonly DependencyProperty ErrorMsgProperty = DependencyProperty.Register(nameof(ErrorMsg), typeof(string), typeof(EImage), new PropertyMetadata(string.Empty, OnErrorMsgChanged));
    public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(nameof(Stretch), typeof(Stretch), typeof(EImage), new FrameworkPropertyMetadata(Stretch.Uniform));
    public static readonly DependencyProperty FallbackAspectRatioProperty = DependencyProperty.Register(nameof(FallbackAspectRatio), typeof(double), typeof(EImage), new FrameworkPropertyMetadata(16/9d, flags: FrameworkPropertyMetadataOptions.AffectsRender, (d, e) => ((EImage)d).InvalidateMeasure()));
    #endregion

    #region actions
    public ICommand RetryCommand
    {
        get => (ICommand)GetValue(RetryCommandProperty);
        set => SetValue(RetryCommandProperty, value);
    }

    public static readonly DependencyProperty RetryCommandProperty = DependencyProperty.Register(nameof(RetryCommand), typeof(ICommand), typeof(EImage), new PropertyMetadata(RetryDefault));
    #endregion

    #endregion

    #region callbacks
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        PART_LoadingOverlay = GetTemplateChild(nameof(PART_LoadingOverlay)) as FrameworkElement;
        PART_ProgressBar = GetTemplateChild(nameof(PART_ProgressBar)) as EProgressBar;
        PART_ErrorNoticeOverlay = GetTemplateChild(nameof(PART_ErrorNoticeOverlay)) as FrameworkElement;
    }
    //url -> task 
    //outer task -> task
    //-> task -> image
    private CancellationTokenSource? cts;
    private static void OnUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EImage imageControl && e.NewValue is string url) StartLoadingImage(imageControl, url);
    }
    private static async void OnRunningTaskChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EImage imageControl && e.NewValue is Task<ImageSource> newTask)
        {
            imageControl.Dispatcher.Invoke(() =>
            {
                imageControl.IsLoading = true;
            });
            try
            {
                var result = await newTask;
                imageControl.Dispatcher.Invoke(() =>
                {
                    imageControl.LoadedImageSource = result;
                    imageControl.ErrorMsg = string.Empty;
                });
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                imageControl.Dispatcher.Invoke(() => imageControl.ErrorMsg = ex.Message);
            }
            finally
            {
                imageControl.Dispatcher.Invoke(() => imageControl.IsLoading = false);
            }
        }
    }

    //
    private FrameworkElement? PART_LoadingOverlay;
    private EProgressBar? PART_ProgressBar;
    private static void OnIsLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EImage imageControl && imageControl.PART_LoadingOverlay is not null && e.NewValue is bool isLoading)
        {
            var overlay = imageControl.PART_LoadingOverlay;
            overlay.Visibility = Visibility.Visible;
            if (isLoading) overlay.Opacity = 0;
            imageControl.PART_ProgressBar?.IsIndeterminate = true;
            var anim = new DoubleAnimation()
            {
                To = isLoading ? 1.0 : 0.0,
                Duration = new Duration(TimeSpan.FromSeconds(isLoading ? imageControl.AnimaDurationIn : imageControl.AnimaDurationOut)),
                EasingFunction = imageControl.AnimaEaseFunc,
                FillBehavior = FillBehavior.Stop
            };

            anim.Completed += (s, e) =>
            {
                if (isLoading) overlay.Opacity = 1;
                else
                {
                    overlay.Visibility = Visibility.Collapsed;
                    imageControl.PART_ProgressBar?.IsIndeterminate = false;
                }
            };
            overlay.BeginAnimation(OpacityProperty, anim);
        }
    }
    private FrameworkElement? PART_ErrorNoticeOverlay;
    private static void OnErrorMsgChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EImage imageControl && imageControl.PART_ErrorNoticeOverlay is not null)
        {
            var overlay = imageControl.PART_ErrorNoticeOverlay;
            if (e.NewValue is string errorMsg && !string.IsNullOrWhiteSpace(errorMsg))
            {
                overlay.Visibility = Visibility.Visible;
                var anim = new DoubleAnimation()
                {
                    To = 1,
                    Duration = new Duration(TimeSpan.FromSeconds(imageControl.AnimaDurationIn)),
                    FillBehavior = FillBehavior.Stop,
                    EasingFunction = imageControl.AnimaEaseFunc
                };
                anim.Completed += (s, e) =>
                {
                    overlay.Opacity = 1;
                };
                overlay.BeginAnimation(OpacityProperty, anim);
            }
            else
            {
                var anim = new DoubleAnimation()
                {
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(imageControl.AnimaDurationOut)),
                    FillBehavior = FillBehavior.Stop,
                    EasingFunction = imageControl.AnimaEaseFunc
                };
                anim.Completed += (s, e) =>
                {
                    overlay.Opacity = 0;
                    overlay.Visibility = Visibility.Collapsed;
                };
                overlay.BeginAnimation(OpacityProperty, anim);
            }
        }
    }
    #endregion

    private static void StartLoadingImage(EImage imageControl, string url)
    {
        imageControl.cts?.Cancel();
        imageControl.cts?.Dispose();

        imageControl.cts = new CancellationTokenSource();

        string newUrl = url;
        if (!string.IsNullOrWhiteSpace(newUrl))
        {
            imageControl.RunningTask = StartLoadingImage(newUrl, imageControl.cts.Token);
        }
        else
        {
            imageControl.RunningTask = null;
            imageControl.IsLoading = false;
            imageControl.ErrorMsg = string.Empty;
        }
    }
    private static async Task<ImageSource> StartLoadingImage(string url, CancellationToken? ct = null)
    {
        var lct = ct ?? CancellationToken.None;
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("Url is null or empty.");
        lct.ThrowIfCancellationRequested();
        var source = await WpfModuleAccessor.ImageSource.GetImageSourceAsync(url);
        lct.ThrowIfCancellationRequested();
        return source ?? throw new Exception($"Failed to get image from \"{url}\".");
    }

    private static VMCommand? retryDefault = null;
    public static VMCommand RetryDefault => retryDefault ??= new(para =>
    {
        Task? t = null;
        if (para is EImage imageControl)
        {
            StartLoadingImage(imageControl, imageControl.Url);
            t = imageControl.RunningTask;
        }
        return t ?? Task.Run(() => { });
    }, null, "RetryGetImageBtnText", "Retry");

    private VMCommand? cancel = null;
    public VMCommand Cancel => cancel ??= new(para =>
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
    }, null, "CancelGetImageBtnText", "Cancel");

    #region measure
    protected override Size MeasureOverride(Size availableSize)
    {
        var pW = Padding.Left + Padding.Right;
        var pH = Padding.Top + Padding.Bottom;
        var contentAvailableW = Math.Max(0, Math.Min(availableSize.Width, MaxWidth) - pW);
        var contentAvailableH = Math.Max(0, Math.Min(availableSize.Height, MaxHeight) - pH);

        var imgDesiredSize = new Size(0, 0);
        var bmp = LoadedImageSource as BitmapSource;
        double intrinsicRatio;
        if (bmp is not null && bmp.PixelWidth > 0 && bmp.PixelHeight > 0)
        {
            intrinsicRatio = bmp.PixelWidth / (double)bmp.PixelHeight;
            switch (Stretch)
            {
                case Stretch.None:
                    imgDesiredSize = new Size(bmp.PixelWidth, bmp.PixelHeight);
                    break;
                case Stretch.Fill:
                    imgDesiredSize = new Size(contentAvailableW, contentAvailableH);
                    break;
                case Stretch.Uniform:
                case Stretch.UniformToFill:
                    var targetRatio = intrinsicRatio;
                    var hByW = contentAvailableW / targetRatio;
                    bool isUniform = Stretch == Stretch.Uniform;
                    if ((isUniform && hByW <= contentAvailableH) || (!isUniform && hByW >= contentAvailableH)) imgDesiredSize = new Size(contentAvailableW, hByW);
                    else imgDesiredSize = new Size(contentAvailableH * targetRatio, contentAvailableH);
                    break;
            }
        }
        else
        {
            intrinsicRatio = FallbackAspectRatio;
            const double ErrorTextMinH = 24;
            if (double.IsInfinity(contentAvailableW) || double.IsInfinity(contentAvailableH))
            {
                var minContentW = Math.Max(0, MinWidth - pW);
                var minContentH = Math.Max(ErrorTextMinH, MinHeight - pH);
                var wByH = minContentH * intrinsicRatio;
                var hByW = minContentW / intrinsicRatio;
                if (wByH >= minContentW && hByW >= minContentH) imgDesiredSize = new Size(minContentW, hByW);
                else imgDesiredSize = new Size(160 * intrinsicRatio, Math.Max(160, minContentH));
            }
            else
            {
                var hByW = contentAvailableW / intrinsicRatio;
                imgDesiredSize = hByW <= contentAvailableH ? new Size(contentAvailableW, hByW) : new Size(contentAvailableH * intrinsicRatio, contentAvailableH);
                imgDesiredSize = new Size(imgDesiredSize.Width, Math.Max(imgDesiredSize.Height, ErrorTextMinH));
            }
        }
        var finalW = Math.Max(imgDesiredSize.Width + pW, MinWidth);
        var finalH = Math.Max(imgDesiredSize.Height + pH, MinHeight);

        if (Stretch == Stretch.Uniform || (!(bmp is not null && bmp.PixelWidth > 0 && bmp.PixelHeight > 0) && intrinsicRatio > 0))
        {
            var finalContentW = finalW - pW;
            var finalContentH = finalH - pH;
            if (finalContentW > imgDesiredSize.Width || finalContentH > imgDesiredSize.Height)
            {
                var newH = finalContentW / intrinsicRatio;
                var newW = finalContentH * intrinsicRatio;
                if (newH * intrinsicRatio >= finalContentW)
                {
                    finalW = finalContentW + pW;
                    finalH = newH + pH;
                }
                else
                {
                    finalW = newW + pW;
                    finalH = finalContentH + pH;
                }
            }
        }
        finalW = Math.Min(finalW, MaxWidth);
        finalH = Math.Min(finalH, MaxHeight);

        if (GetVisualChild(0) is UIElement ele) ele.Measure(imgDesiredSize);

        if (double.IsInfinity(finalW))
            finalW = 0;
        if (double.IsInfinity(finalH))
            finalH = 0;

        return new Size(finalW, finalH);
    }

    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        var desiredSize = DesiredSize;
        double finalWidth = Math.Min(arrangeBounds.Width, desiredSize.Width);
        double finalHeight = Math.Min(arrangeBounds.Height, desiredSize.Height);
        var finalRect = new Rect(0, 0, finalWidth, finalHeight);
        if (GetVisualChild(0) is UIElement visualChild) visualChild.Arrange(finalRect);
        return new Size(finalWidth, finalHeight);
    }
    #endregion
}
