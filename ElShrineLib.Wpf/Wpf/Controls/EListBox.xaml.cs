using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.Controls.Extensions;
using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ElShrine.Wpf.Controls;

[GenerateDPCli]
public partial class EListBox : ListBox, IThemeControlBase, IScrollBarControlBase, ISelectionRenderControlBase, IScrollBarControllerBase, IItemRenderControlBase
{

    #region Implements
    static EListBox() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EListBox), new FrameworkPropertyMetadata(typeof(EListBox)));
    public EListBox()
    {
        WpfModuleAccessor.UITheme.RegisterCoerceThemeDPs(this);
        MouseMove += (s, e) =>
        {
            if (!isDragging && Mouse.LeftButton == MouseButtonState.Pressed) isDragging = true;
        };
        PreviewMouseUp += ListBox_PreviewMouseUp;
    }
    public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => TransHelper.CoerceValue(this);
    public void LocalThemePorpertyChanged(DependencyPropertyChangedEventArgs e) => WpfModuleAccessor.StateListener.RedoSetterTransitions(this);
    #endregion

    protected override DependencyObject GetContainerForItemOverride() => new EListBoxItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is EListBoxItem;

    private bool isDragging = false;
    private void ListBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (isDragging)
        {
            var ins = WpfModuleAccessor.StateListener;
            foreach (var item in Items)
            {
                var container = ItemContainerGenerator.ContainerFromItem(item);
                ins.RevaluateStatus(container);
            }
            isDragging = false;
        }
    }
}
