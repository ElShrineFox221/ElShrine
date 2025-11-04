using ElShrine.EOption;
using ElShrine.Wpf.UITheme;

namespace ElShrine.Wpf
{
    [Option(Name = "Wpf")]
    public sealed class WpfOption : ISingleton<WpfOption>
    {
        private static WpfOption? Instance = null;
        private WpfOption() { }
        public static WpfOption GetInstance() => Instance ??= new();

        #region Theme
        [OptionItem(Ignored = true)] public Theme SelectedTheme { get; set; } = Theme.Default;
        [OptionItem] public int SelectedThemeIndex { get; set; } = -1;
        #endregion


        #region Window animation
        public int FadeInOutms { get; set; } = 300;
        #endregion
    }
}
