using ElShrine.EGraphic;
using ElShrine.Modules.OptionHub.ViewModel;
using ElShrine.Wpf;
using ElShrine.Wpf.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ElShrine.Modules.OptionHub.View
{
    public partial class ThemeEditorView
    {
        private Popup? colorPalettePopup = null;
        private EColorPalette? colorPalette = null;
        private EComboBox? themeComboBox;
        private ThemeEditorVM? viewModel = null;
        private void ViewElementLoaded(object sender, RoutedEventArgs e)
        {
            if(viewModel is null && sender is FrameworkElement fe)
            {
                var parent = fe.FindParent<ContentControl>();
                if (parent is not null && parent.DataContext is ThemeEditorVM tvm) viewModel = tvm;
            }
            if (sender is Popup p) colorPalettePopup = p;
            else if (sender is EColorPalette cp) colorPalette = cp;
            else if (sender is EComboBox cb) themeComboBox = cb;
        }
        private string currentColorCarrierName = string.Empty;
        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(sender is Border bd && colorPalettePopup is not null)
            {
                colorPalettePopup.PlacementTarget = bd;
                colorPalettePopup.IsOpen = !colorPalettePopup.IsOpen;
                if (colorPalette is not null && bd.Background is SolidColorBrush scb) colorPalette.SetInitialColor(scb.Color.ToColorData());
                currentColorCarrierName = bd.Name;
            }
        }

        private void EColorPalette_OnResultColorUpdated(ColorData oldData, ColorData newData, ControlDataUpdates updates)
        {
            if (updates != ControlDataUpdates.Update)
            {
                if (colorPalettePopup is not null) colorPalettePopup.IsOpen = false;
            }
            viewModel?.UpdateColor(newData, currentColorCarrierName);
        }

        private void EComboBox_DropDownOpened(object sender, EventArgs e)
        {
            if(sender is ComboBox cb && viewModel is not null)
            {
                viewModel.UpdateThemes();
            }
        }
    }
}
