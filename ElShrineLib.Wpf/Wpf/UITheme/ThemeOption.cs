using ElShrine.EOption;

namespace ElShrine.Wpf.UITheme
{
    [Option(Name = "Theme")]
    public sealed class ThemeOption : ISingleton<ThemeOption>
    {
        private static ThemeOption? Instance = null;
        public static ThemeOption GetInstance() => Instance ??= new();

        [OptionItem(Ignored = true)] public Theme SelectedTheme { get; set; } = Theme.Default;
        [OptionItem] public int SelectedThemeIndex { get; set; } = -1; 
    }
}
