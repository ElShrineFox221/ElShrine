using ElShrine.Modules.Option;
using ElShrine.Wpf.UITheme;

namespace ElShrine.Wpf;

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
