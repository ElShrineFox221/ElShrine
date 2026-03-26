using ElShrine.Wpf.Controls.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ElShrine.Wpf.Controls
{
    [GenerateDPCli]
    public partial class EArrowControl : Control, IArrowControllerBase
    {
        #region Implements
        static EArrowControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EArrowControl), new FrameworkPropertyMetadata(typeof(EArrowControl)));
            ArrowBasicAngleProperty.OverrideMetadata(typeof(EArrowControl), 
                new FrameworkPropertyMetadata(0d, propertyChangedCallback: ArrowBasicAngleChanged));
            ArrowTargetAngleProperty.OverrideMetadata(typeof(EArrowControl), 
                new FrameworkPropertyMetadata(90d, propertyChangedCallback: ArrowTargetAngleChanged));
        }
        #endregion

        public bool IsBasicRotation { get; protected set; } = true;

        private RotateTransform? PART_ArrowRotateTransform;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            PART_ArrowRotateTransform = GetTemplateChild(nameof(PART_ArrowRotateTransform)) as RotateTransform;
            DoRotate(IsBasicRotation ? ArrowBasicAngle : ArrowTargetAngle);
        }
        public void DoRotate(double targetAngle = double.NaN)
        {
            if(PART_ArrowRotateTransform is null) return;
            var currentAngle = PART_ArrowRotateTransform.Angle;
            var isToBasicAngle = currentAngle != ArrowBasicAngle;
            var tarAngle = double.IsRealNumber(targetAngle) ? targetAngle : (isToBasicAngle ? ArrowBasicAngle : ArrowTargetAngle);
            if(currentAngle == tarAngle) return;
            TransHelper.GetThemeControlParent(this, out _, out var themeControl);
            var anim = themeControl.ToDoubleAnimation(tarAngle, !isToBasicAngle);
            anim.Completed += (s, e) =>
            {
                IsBasicRotation = isToBasicAngle;
            };
            PART_ArrowRotateTransform.BeginAnimation(RotateTransform.AngleProperty, anim);
        }

        private static void ArrowBasicAngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is EArrowControl control && control.IsBasicRotation && (double)e.OldValue != (double)e.NewValue) 
                control.DoRotate((double)e.NewValue);
        }
        private static void ArrowTargetAngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is EArrowControl control && !control.IsBasicRotation && (double)e.OldValue != (double)e.NewValue) 
                control.DoRotate((double)e.NewValue);
        }
    }
}
