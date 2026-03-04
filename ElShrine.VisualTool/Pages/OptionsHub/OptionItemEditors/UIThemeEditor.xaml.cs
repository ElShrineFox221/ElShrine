using ElShrine.Graphics;
using ElShrine.Modules;
using ElShrine.Wpf.UITheme;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ElShrine.VisualTool.Pages.OptionsHub.OptionItemEditors
{
    [OptionItemEditor]
    public partial class UIThemeEditor : UserControl, IOptionItemEditor
    {
        private readonly ObservableCollection<Theme> Themes;
        private bool currentIsDefaultTheme;
        public UIThemeEditor()
        {
            InitializeComponent();
            Themes = [.. UIThemesManager.Instance.AllThemes];
            //Update data source
            UIThemesManager.Instance.ThemesChanged += (s, e) =>
            {
                var ins = UIThemesManager.Instance;
                Themes.ReplaceAll(ins.AllThemes);
            };
            UIThemesManager.Instance.CurrentThemeChanged += (s, e) => 
            {
                currentIsDefaultTheme = UIThemesManager.Instance.CurrentTheme == Theme.Default;
                PART_SubEditorsGrid.IsEnabled = !currentIsDefaultTheme;
            };
            //Update view - binding
            var binding = new Binding()
            {
                Source = Themes
            };
            PART_ThemeSelectionBox.SetBinding(ItemsControl.ItemsSourceProperty, binding);
            PART_ThemeSelectionBox.SelectedItem = UIThemesManager.Instance.CurrentTheme;
            //
            PART_Palette.Confirmed += (s, e) =>
            {
                PART_PalettePopup.IsOpen = false;
                var utmct = UIThemesManager.Instance.CurrentTheme;
                var rc = PART_Palette.ResultColor;
                switch (cached_TargetProp)
                {
                    case ThemeProperty.PrimaryBrush:
                        utmct.PrimaryColor = rc.Clone();
                        break;
                    case ThemeProperty.BackBrush:
                        utmct.BackColor = rc.Clone();
                        break;
                    case ThemeProperty.SecondaryBrush:
                        utmct.SecondaryColor = rc.Clone();
                        break;
                    case ThemeProperty.FontBrush:
                        utmct.FontColor = rc.Clone();
                        break;
                }
            };
            PART_Palette.Canceled += (s, e) =>
            {
                PART_PalettePopup.IsOpen = false;
            };
            //
            PART_NewThemeBtn.Click += (s, e) =>
            {
                var newName = PART_NewThemeNameBox.Text;
                var ins = UIThemesManager.Instance;
                if (newName.IsEmpty()) newName = "NewTheme";
                if (ins.AllThemes.Any(th => th.ThemeName.EqualIgnoreCase(newName))) newName = $"{newName}_New";
                ins.NewTheme(newName);
                PART_ThemeSelectionBox.SelectedIndex = 0;
            };
            PART_ThemeSelectionBox.SelectionChanged += (s, e) =>
            { 
                if(PART_ThemeSelectionBox.SelectedItem is Theme theme && theme != UIThemesManager.Instance.CurrentTheme)
                    UIThemesManager.Instance.CurrentTheme = theme;
            };
        }

        public void RegisterUpdateValue(OptionItem source, Action<object?> updatedValueFromUI, out Action<object?> updateValueToUI)
        {
            PART_ThemeSelectionBox.SelectionChanged += (s, e) =>
            {
                updatedValueFromUI(PART_ThemeSelectionBox.SelectedItem);
            };
            updateValueToUI = (v) =>
            {
                var str = v?.ToString() ?? string.Empty;
            };
        }

        private bool cached_IsOpen;
        private WeakReference<UIElement>? cached_Container;
        private ThemeProperty cached_TargetProp = ThemeProperty.AnimaDurationIn;
        private void PART_ColorContainer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is not ContentControl container) return;
            if (container.Content is not ColorData cd) return;
            //
            UpdateCacheInfo(sender);
            //
            //if (oldEle != newEle) PART_PalettePopup.IsOpen = true;
            if (!cached_IsOpen) 
            {
                PART_PalettePopup.IsOpen = true;
                PART_Palette.Open(cd);
            }
            cached_IsOpen = PART_PalettePopup.IsOpen;
        }
        private void PART_ColorContainer_PreMouseDown(object sender, MouseButtonEventArgs e)
        {
            UpdateCacheInfo(sender);
            cached_IsOpen = PART_PalettePopup.IsOpen;
        }
        private void UpdateCacheInfo(object sender)
        {
            var uie = sender as UIElement;
            PART_PalettePopup.PlacementTarget = uie;
            cached_Container = uie is null ? null : new(uie);
            if(uie is ContentControl cc && cc.Content is ColorData cd)
            {
                var utmct = UIThemesManager.Instance.CurrentTheme;
                if(cd == utmct.PrimaryColor) cached_TargetProp = ThemeProperty.PrimaryBrush;
                else if(cd == utmct.BackColor) cached_TargetProp = ThemeProperty.BackBrush;
                else if(cd == utmct.SecondaryColor) cached_TargetProp = ThemeProperty.SecondaryBrush;
                else if(cd == utmct.FontColor) cached_TargetProp = ThemeProperty.FontBrush;
                else cached_TargetProp = ThemeProperty.AnimaDurationIn;
            }
        }
    }
}
