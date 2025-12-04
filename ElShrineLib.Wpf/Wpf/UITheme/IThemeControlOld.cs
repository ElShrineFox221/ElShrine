using System.Windows;
using System.Windows.Media;

namespace ElShrine.Wpf.UITheme
{
    public interface IThemeControlOld
    {
        public CornerRadius BorderCornerRadius { get; set; }
        public Thickness BorderThickness { get; set; }
        public Brush BorderBrush { get; set; }
        public Brush FontBrush { get; set; }
        public Brush SelectionBrush { get; set; }
        public Brush ClickBrush { get; set; }
    }
}
