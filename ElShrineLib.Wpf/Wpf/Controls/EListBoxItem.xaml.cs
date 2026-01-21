using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.Controls.Extensions;
using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls;

namespace ElShrine.Wpf.Controls
{
    [GenerateDPCli]
    public partial class EListBoxItem : ListBoxItem, IThemeControlBase, ISelectionRenderControlBase
    {
        #region DPs
        public double TransNorm
        {
            get => (double)GetValue(TransNormProperty);
        }
        protected readonly static DependencyProperty TransNormProperty = DependencyProperty.Register(nameof(TransNorm), typeof(double), typeof(EListBoxItem), new(0d));
        #endregion

        #region Implements
        static EListBoxItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EListBoxItem), new FrameworkPropertyMetadata(typeof(EListBoxItem)));
            IsSelectedProperty.OverrideMetadata(typeof(EListBoxItem), new FrameworkPropertyMetadata(false, (s, e) =>
            {
                if (s is EListBoxItem elbi)
                {
                    var selected = (bool)e.NewValue;
                    var tc = elbi as IThemeControlBase;
                    var anim = tc.ToDoubleAnimation(selected ? 1 : 0, !selected);
                    elbi.BeginAnimation(TransNormProperty, anim);
                }
            }));
        }
        public EListBoxItem() => UIThemesManager.RegisterCoerceThemeDPs(this);
        public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => UIThemesManager.CoerceValue(this);
        public void LocalThemePorpertyChanged(DependencyPropertyChangedEventArgs e) => StateListenersManager.Instance.RedoSetterTransitions(this);
        #endregion
    }
}
