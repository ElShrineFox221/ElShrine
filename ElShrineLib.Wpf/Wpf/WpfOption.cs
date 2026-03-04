using ElShrine.Modules;
using ElShrine.Wpf.UITheme;

namespace ElShrine.Wpf
{
    [Option]
    public static class WpfOption
    {
        #region Theme
        [OptionItem]
        public static Theme SelectedTheme { get; set; } = Theme.Default;
        #endregion


        #region Window animation
        public static int FadeInOutms { get; set; } = 300;
        #endregion
    }
}
