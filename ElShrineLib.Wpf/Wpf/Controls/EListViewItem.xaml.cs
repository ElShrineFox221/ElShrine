using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.Controls.Extensions;
using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls;

namespace ElShrine.Wpf.Controls
{
    [GenerateDPCli]
    public partial class EListViewItem : ListBoxItem, IThemeControlBase, ISelectionRenderControlBase
    {
        #region DPs
        public double TransNorm
        {
            get => (double)GetValue(TransNormProperty);
        }
        protected readonly static DependencyProperty TransNormProperty = DependencyProperty.Register(nameof(TransNorm), typeof(double), typeof(EListViewItem), new(0d));
        #endregion

        #region Implements
        static EListViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EListViewItem), new FrameworkPropertyMetadata(typeof(EListViewItem)));
            IsSelectedProperty.OverrideMetadata(typeof(EListViewItem), new FrameworkPropertyMetadata(false, (s, e) =>
            {
                if (s is EListViewItem elbi)
                {
                    var selected = (bool)e.NewValue;
                    var tc = elbi as IThemeControlBase;
                    var anim = tc.ToDoubleAnimation(selected ? 1 : 0, !selected);
                    elbi.BeginAnimation(TransNormProperty, anim);
                }
            }));
        }
        public EListViewItem() => WpfModuleAccessor.UITheme.RegisterCoerceThemeDPs(this);
        public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => TransHelper.CoerceValue(this);
        public void LocalThemePorpertyChanged(DependencyPropertyChangedEventArgs e) => WpfModuleAccessor.StateListener.RedoSetterTransitions(this);
        #endregion
    }
}
