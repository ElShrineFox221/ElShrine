using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.Controls.Extensions;
using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls;

namespace ElShrine.Wpf.Controls
{
    [GenerateDPCli]
    public partial class ESelectedIndicator : ContentControl, IThemeControlBase, ISelectionRenderControlBase
    {
        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }
        public double TransNormSelectionProgress
        {
            get => (double)GetValue(TransNormSelectionProgressProperty);
        }
        public readonly static DependencyProperty IsSelectedProperty = DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(ESelectedIndicator), new FrameworkPropertyMetadata(false, propertyChangedCallback: OnIsSelectedChanged));
        public readonly static DependencyProperty TransNormSelectionProgressProperty = DependencyProperty.Register(nameof(TransNormSelectionProgress), typeof(double), typeof(ESelectedIndicator), new(0d));
        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ESelectedIndicator ele && e.NewValue is bool nb && e.OldValue is bool ob)
            {
                if(nb ^ ob)
                {
                    var anim = ele.ToDoubleAnimation(nb ? 1 : 0, nb);
                    ele.BeginAnimation(TransNormSelectionProgressProperty, anim);
                }
            }
        }

        #region Implements
        static ESelectedIndicator() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ESelectedIndicator), new FrameworkPropertyMetadata(typeof(ESelectedIndicator)));
        public ESelectedIndicator() => WpfModuleAccessor.UITheme.RegisterCoerceThemeDPs(this);
        public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => TransHelper.CoerceValue(this);
        public void LocalThemePorpertyChanged(DependencyPropertyChangedEventArgs e) => WpfModuleAccessor.StateListener.RedoSetterTransitions(this);
        #endregion
    }
}
