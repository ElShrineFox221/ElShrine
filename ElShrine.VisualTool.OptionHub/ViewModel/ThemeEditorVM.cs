using ElShrine.EGraphic;
using ElShrine.Wpf.UITheme;
using ElShrine.Wpf.ViewModel;
using System.Collections.ObjectModel;
using System.Windows;
using VMC = ElShrine.Wpf.VMCommand;

namespace ElShrine.VisualTool.OptionHub.ViewModel
{
    public class ThemeEditorVM : ViewModelBase
    {
        public const string DataTemplateUri = "/ElShrienVisualTool;component/Modules/OptionHub/View/ThemeEditorView.xaml";
        public const string DataTemplateName = "ThemeEditorTemplate";
        private readonly static ThemeManager themeManager = ThemeManager.GetInstance();

        public ThemeEditorVM()
        {
            SelectedTheme = Theme.Default;
        }

        public static VMC NewThemeCommand => new(parameter =>
        {
            var r = MessageBox.Show("test", "title", MessageBoxButton.OKCancel);
            
            var themeName = parameter?.ToString() ?? "New Theme";
            Themes.Add(new() {  });
        });
        public static VMC DeleteThemeCommand => new(parameter =>
        {
            var target = Themes.ToList().Find(t => t.ThemeName.EqualIgnoreCase(parameter?.ToString() ?? string.Empty));
            if(target is not null) Themes.Remove(target);
        });
        public static ObservableCollection<Theme> Themes => [..themeManager.LoadedThemes];
        public void UpdateThemes() => NoticePropertyChanged(nameof(Themes));

        public Theme SelectedTheme { get; set; }
        public bool IsEditable => SelectedTheme != Theme.Default;
        public void UpdateColor(ColorData newValue, string propName)
        {
            if (propName.ContainsIgnoreCase("fore")) SelectedTheme.ForeColor = newValue;
            else if (propName.ContainsIgnoreCase("back")) SelectedTheme.BackColor = newValue;
            else if (propName.ContainsIgnoreCase("font")) SelectedTheme.FontColor = newValue;
            else if (propName.ContainsIgnoreCase("selection")) SelectedTheme.SelectionColor = newValue;
        }
    }
}
