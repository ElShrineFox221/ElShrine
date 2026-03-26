using ElShrine.Wpf.Controls.Extensions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ElShrine.Wpf.Controls
{
    [GenerateDPCliDeclares(DefaultDPOwnerType = typeof(ScaleControllerProperties))]
    public interface IScaleControllerBase
    {
        double Scale { get; set; }
        bool IsShakeMode { get; set; }
        bool BeTriggered { get; set; }
        bool IsPersistantScale { get; set; }
        ScaleTransform ScaleTarget { get; set; }
    }
    public static class ScaleControllerProperties
    {
        #region DPs
        public static readonly DependencyProperty ScaleProperty = DependencyProperty.RegisterAttached(
            nameof(ScaleProperty).ToPropRegName(),
            typeof(double),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: 0.8d,
                flags: FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure,
                propertyChangedCallback: OnScaleOffsetRateChanged
        ));
        public static readonly DependencyProperty IsShakeModeProperty = DependencyProperty.RegisterAttached(
            nameof(IsShakeModeProperty).ToPropRegName(),
            typeof(bool),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: false,
                flags: FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure,
                propertyChangedCallback: OnIsShakeModeChanged
        ));
        public static readonly DependencyProperty ScaleTargetProperty = DependencyProperty.RegisterAttached(
            nameof(ScaleTargetProperty).ToPropRegName(),
            typeof(ScaleTransform),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: null,
                propertyChangedCallback: OnScaleTargetChanged
        ));
        public static readonly DependencyProperty BeTriggeredProperty = DependencyProperty.RegisterAttached(
            nameof(BeTriggeredProperty).ToPropRegName(),
            typeof(bool),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: false,
                flags: FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsMeasure,
                propertyChangedCallback: OnBeTriggeredChanged
        ));
        public static readonly DependencyProperty IsPersistantScaleProperty = DependencyProperty.RegisterAttached(
            nameof(IsPersistantScaleProperty).ToPropRegName(),
            typeof(bool),
            typeof(UIElement),
            new FrameworkPropertyMetadata(
                defaultValue: false
        ));
        #endregion
        private static void OnScaleOffsetRateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is IScaleControllerBase { BeTriggered: true, IsShakeMode: false, ScaleTarget: ScaleTransform st })
            {
                TransHelper.GetThemeControlParent(d, out _, out var tc);
                var anim = tc.ToDoubleAnimation((double)e.NewValue, true);
                st.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
                st.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
            }
        }
        private static void OnIsShakeModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is IScaleControllerBase { BeTriggered: true, ScaleTarget: ScaleTransform st })
            {
                TransHelper.GetThemeControlParent(d, out _, out var tc);
                var anim = tc.ToDoubleAnimation(1, false);
                st.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
                st.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
            }
        }
        private static void OnScaleTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue == e.OldValue) return;
            if(d is IScaleControllerBase scb && scb.BeTriggered && !scb.IsShakeMode)
            {
                TransHelper.GetThemeControlParent(d, out _, out var tc);
                if (e.OldValue is ScaleTransform ost)
                {
                    var anim = tc.ToDoubleAnimation(1, false);
                    ost.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
                    ost.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
                }
                if(e.NewValue is ScaleTransform nst)
                {
                    var anim = tc.ToDoubleAnimation(scb.Scale, true);
                    nst.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
                    nst.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
                }
            }
        }
        
        private static void OnBeTriggeredChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        { 
            if(d is IScaleControllerBase scb && scb.ScaleTarget is ScaleTransform st)
            {
                if (d is UIElement { IsEnabled: false }) return;

                TransHelper.GetThemeControlParent(d, out _, out var tc);
                var triggered = (bool)e.NewValue;
                if (scb.IsShakeMode)
                {
                    if(!triggered) return;
                    var completed = false;
                    var animIn = tc.ToDoubleAnimation(scb.Scale, true);
                    var animOut = tc.ToDoubleAnimation(1, false);
                    animIn.Completed += (s, e) =>
                    {
                        if (completed) return;
                        completed = true;
                        st.BeginAnimation(ScaleTransform.ScaleXProperty, animOut);
                        st.BeginAnimation(ScaleTransform.ScaleYProperty, animOut);
                    };
                    st.BeginAnimation(ScaleTransform.ScaleXProperty, animIn);
                    st.BeginAnimation(ScaleTransform.ScaleYProperty, animIn);
                }
                else
                {
                    var anim = tc.ToDoubleAnimation(triggered ? scb.Scale : 1, !triggered);
                    st.BeginAnimation(ScaleTransform.ScaleXProperty, anim, HandoffBehavior.SnapshotAndReplace);
                    st.BeginAnimation(ScaleTransform.ScaleYProperty, anim, HandoffBehavior.SnapshotAndReplace);
                }
            }
        }
    }
    public partial class ScaleController : FrameworkElement, IScaleControllerBase
    {
        static ScaleController()
        {
            IsEnabledProperty.OverrideMetadata(typeof(ScaleController), new FrameworkPropertyMetadata(defaultValue: true,propertyChangedCallback: OnIsEnabledChanged));
        }
        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue && d is IScaleControllerBase scb && !scb.IsPersistantScale && scb.ScaleTarget is ScaleTransform st)
            {
                TransHelper.GetThemeControlParent(d, out _, out var tc);
                var anim = tc.ToDoubleAnimation(1, false);
                st.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
                st.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
            }
        }
    }
}
