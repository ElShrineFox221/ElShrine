using ElShrine.Modules;
using ElShrine.Wpf.UITheme;

namespace ElShrine.Wpf
{
    [Option]
    public static class WpfOption
    {
        #region Theme
        public static Theme SelectedTheme { get; set; } = Theme.Default;
        [OptionItem] public static int SelectedThemeIndex { get; set; } = -1;
        #endregion


        #region Window animation
        public static int FadeInOutms { get; set; } = 300;
        #endregion
    }
}
