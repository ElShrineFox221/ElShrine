using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls;

namespace ElShrine.Wpf.Controls
{
    [GenerateDPCli]
    public partial class ETreeView : TreeView, IThemeControlBase, IItemRenderControlBase, IScrollBarControllerBase
    {
        #region DPs
        public double IndentUnitLength
        {
            get => (double)GetValue(IndentUnitLengthProperty);
            set => SetValue(IndentUnitLengthProperty, value);
        }
        public Thickness ArrowMargin
        {
            get => (Thickness)GetValue(ArrowMarginProperty);
            set => SetValue(ArrowMarginProperty, value);
        }
        public static readonly DependencyProperty IndentUnitLengthProperty = DependencyProperty.Register(nameof(IndentUnitLength), typeof(double), typeof(ETreeView), new(16d));
        public static readonly DependencyProperty ArrowMarginProperty = DependencyProperty.Register(nameof(ArrowMargin), typeof(Thickness), typeof(ETreeView), new(new Thickness(0d)));
        #endregion

        #region Implements
        static ETreeView() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ETreeView), new FrameworkPropertyMetadata(typeof(ETreeView)));
        public ETreeView() => UIThemesManager.RegisterCoerceThemeDPs(this);
        public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => UIThemesManager.CoerceValue(this);
        public void LocalThemePorpertyChanged(DependencyPropertyChangedEventArgs e) => StateListenersManager.Instance.RedoSetterTransitions(this);
        #endregion

        protected override DependencyObject GetContainerForItemOverride() => new ETreeViewItem();
        protected override bool IsItemItsOwnContainerOverride(object item) => item is ETreeViewItem;
    }
}
