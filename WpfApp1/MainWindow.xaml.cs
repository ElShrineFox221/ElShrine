using ElShrine;
using ElShrine.Common;
using ElShrine.Wpf.Controls;
using System.Windows;
using System.Windows.Media;
using MediaColor = System.Windows.Media.Color;
using DrawingColor = System.Drawing.Color;
using ElShrine.Wpf;
using ElShrine.Graphics;
using ElShrine.Modules;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : EWindow
    {
        public MainWindow()
        {
            
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TestBtn.PrimaryBrush = Brushes.Red;
        }
        private readonly static Random random = new();
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            UIThemesManager.Instance.CurrentTheme.PrimaryColor = ColorData.FromData((uint)random.Next(int.MinValue, int.MaxValue));
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            UIThemesManager.Instance.CurrentTheme.FontSizeNormal = random.Next(12, 30);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            TestControl1.Url = TestTextBox.Text;
        }
        public List<string> Suggestions { get; set; } =
        [
            "alpha", "arch", "activity",
            "beta", "gamma", "delta",
            "epsilon", "zeta", "theta",
            "lambda", "sigma", "omega",
            "architecture", "archive", "archer",
            "architect", "archway", "archaic",
            "active", "action", "actor",
            "activate", "actual", "acoustic",
            "algorithm", "algebra", "alphabet",
            "analysis", "analog", "android",
            "application", "apparatus", "approach",
            "database", "digital", "dynamic",
            "element", "energy", "engine",
            "function", "framework", "future"
        ];
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            TestETextBox.SuggestionsSource = Suggestions;
        }

        private void ESelectedIndicator_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            
        }

        private void TestCBB_Loaded(object sender, RoutedEventArgs e)
        {
            if(sender is EComboBox cbb)
            {
                cbb.ItemsSource = Suggestions;
            }
        }

        private void TestBtn3_Click(object sender, RoutedEventArgs e)
        {
            TestETB.IsEnabled = !TestETB.IsEnabled;
        }
    }
}