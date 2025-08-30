using ElShrine.Old.Wpf.ViewModel;
using ElShrine.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace ElShrine.Old.Wpf.Controls
{
    public partial class ColorPalette : UserControl
    {
        public ColorVM Color
        {
            get { return (ColorVM)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(ColorVM), typeof(ColorPalette), new PropertyMetadata(new ColorVM(System.Drawing.Color.PaleVioletRed.ToMediaColor())));

        public ColorPalette()
        {
            InitializeComponent();
        }
    }
}