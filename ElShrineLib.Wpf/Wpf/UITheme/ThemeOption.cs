using ElShrine.EOption;

namespace ElShrine.Wpf.UITheme
{
    [Option(Name = "Theme")]
    public sealed class ThemeOption : ISingleton<ThemeOption>
    {
        private static ThemeOption? Instance = null;
        public static ThemeOption GetInstance() => Instance ??= new();

        [OptionItem] public Theme SelectedTheme { get; set; } = Theme.Default;
    }
}
