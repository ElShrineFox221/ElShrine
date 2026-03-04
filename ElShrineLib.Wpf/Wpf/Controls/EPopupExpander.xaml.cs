using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ElShrine.Wpf.Controls
{
    [GenerateDPCli]
    public partial class EPopupExpander : Expander, IThemeControlBase, IItemRenderControlBase, IHeaderControlBase, IArrowControllerBase
    {
        #region DPs
        public bool StaysOpen
        {
            get => (bool)GetValue(StaysOpenProperty);
            set => SetValue(StaysOpenProperty, value);
        }
        public HeaderPlacement ArrowPlacement
        {
            get => (HeaderPlacement)GetValue(ArrowPlacementProperty);
            protected set => SetValue(ArrowPlacementProperty, value);
        }

        public static readonly DependencyProperty StaysOpenProperty = DependencyProperty.Register(nameof(StaysOpen), typeof(bool), typeof(EPopupExpander), new(false));
        public static readonly DependencyProperty ArrowPlacementProperty = DependencyProperty.Register(nameof(ArrowPlacement), typeof(HeaderPlacement), typeof(EPopupExpander), new(HeaderPlacement.Right));
        #endregion

        #region Implements
        static EPopupExpander()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EPopupExpander), new FrameworkPropertyMetadata(typeof(EPopupExpander)));
            IsExpandedProperty.OverrideMetadata(typeof(EPopupExpander), new FrameworkPropertyMetadata(defaultValue: false, propertyChangedCallback: IsExpandedChanged));
            HeaderPlacementProperty.OverrideMetadata(typeof(EPopupExpander), new FrameworkPropertyMetadata(defaultValue: HeaderPlacement.Left, propertyChangedCallback: (s, e) =>
            {
                if (s is EPopupExpander expander && e.NewValue is HeaderPlacement hPlacement)
                {
                    expander.ArrowPlacement = hPlacement switch
                    {
                        HeaderPlacement.Left => HeaderPlacement.Right,
                        HeaderPlacement.Right => HeaderPlacement.Left,
                        HeaderPlacement.Top => HeaderPlacement.Bottom,
                        HeaderPlacement.Bottom => HeaderPlacement.Top,
                        HeaderPlacement.LeftTop => HeaderPlacement.RightBottom,
                        HeaderPlacement.RightTop => HeaderPlacement.LeftBottom,
                        HeaderPlacement.LeftBottom => HeaderPlacement.RightTop,
                        HeaderPlacement.RightBottom => HeaderPlacement.LeftTop,
                        _ => HeaderPlacement.Right,
                    };
                }
            }));
        }
        public EPopupExpander() => UIThemesManager.RegisterCoerceThemeDPs(this);
        public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => UIThemesManager.CoerceValue(this);
        public void LocalThemePorpertyChanged(DependencyPropertyChangedEventArgs e) => StateListenersManager.Instance.RedoSetterTransitions(this);
        #endregion

        private EArrowControl? PART_ArrowContainer;
        private ToggleButton? PART_TOGBTN;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            PART_ArrowContainer = GetTemplateChild(nameof(PART_ArrowContainer)) as EArrowControl;
            PART_TOGBTN = GetTemplateChild(nameof(PART_TOGBTN)) as ToggleButton;

            PART_TOGBTN?.PreviewMouseUp += (s, e) =>
            {
                if (LastStateIsExpanded && !StaysOpen)
                {
                    e.Handled = true;
                    PART_TOGBTN.ReleaseMouseCapture();
                }
            };
        }
        private static void IsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EPopupExpander expander && ((bool)e.NewValue ^ (bool)e.OldValue) && expander.PART_ArrowContainer is not null)
                expander.PART_ArrowContainer.DoRotate((bool)e.NewValue ? expander.PART_ArrowContainer.ArrowTargetAngle : expander.PART_ArrowContainer.ArrowBasicAngle);
        }

        private bool LastStateIsExpanded = false;
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            LastStateIsExpanded = IsExpanded;
        }
    }
}
