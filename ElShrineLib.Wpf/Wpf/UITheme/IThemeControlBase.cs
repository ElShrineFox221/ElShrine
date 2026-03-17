using ElShrine.Common;
using ElShrine.Modules;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ElShrine.Wpf.UITheme
{
    [GenerateDPCliDeclares(DefaultDPOwnerType = typeof(ThemeProperties))]
    public interface IThemeControlBase
    {
        #region Border base
        public CornerRadius EBorderCornerRadius { get; set; }
        public Thickness EBorderThickness { get; set; }
        #endregion

        #region Color base
        public Brush PrimaryBrush { get; set; }
        public Brush FontBrush { get; set; }
        public Brush BackBrush { get; set; }
        public Brush SecondaryBrush { get; set; }
        #endregion

        #region Anima base
        public double AnimaDurationIn { get; set; }
        public double AnimaDurationOut { get; set; }

        public EasingFunctionBase AnimaEaseFunc { get; set; }
        #endregion

        void LocalThemePorpertyChanged(DependencyPropertyChangedEventArgs e);
        void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e);
    }
}
