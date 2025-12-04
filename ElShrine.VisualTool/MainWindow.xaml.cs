using ElShrine.Debug;
using ElShrine.EConsole;
using ElShrine.VisualTool.Modules.Console.ViewModel;
using ElShrine.Wpf;
using ElShrine.Wpf.Controls;
using System.Windows;
using System.Windows.Controls;

namespace ElShrine.VisualTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : EWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            
            Loaded += (_, _) =>
            {
                DebugConsoleProgram.Initilize();
                var mainWindow = EWindowViewModelBase.SetWindowViewModel<MainWindowViewModel>(this);
                mainWindow.ModuleManager.TabsController = ModulesTabControl;
                ConsoleManager.DefaultSub = true;
                WpfPageManager.Refresh();
                ConsoleManager.DefaultSub = false;
                
                ConsoleManager.SetController(new SystemConsoleController());
            };
        }
        private void EnabledModulesListView_DragItemDropped(object sender, object item, ListView source)
        {
            if(item is WpfPageInfoVM mvm)
            {
                mvm.Enabled = true;
                if (sender is FrameworkElement fe && fe.DataContext is MainWindowViewModel mwvm)
                {
                    mwvm.ModuleManager.TabsController = ModulesTabControl;
                    mwvm.ModuleManager.RefreshIndexes();
                }
            }
        }
        private void DisabledModulesListView_DragItemDropped(object sender, object item, ListView source)
        {
            if (item is WpfPageInfoVM mvm)
            {
                mvm.Enabled = false;
                if (sender is FrameworkElement fe && fe.DataContext is MainWindowViewModel mwvm)
                {
                    mwvm.ModuleManager.TabsController = ModulesTabControl;
                    mwvm.ModuleManager.RefreshIndexes();
                }
            }
        }

        private void testSwitchEnabledBtn_Click(object sender, RoutedEventArgs e)
        {
            testImage.IsEnabled = !testImage.IsEnabled;
            testImage1.IsEnabled = !testImage1.IsEnabled;
        }

        private void testConfrimBtn_Click(object sender, RoutedEventArgs e)
        {
            testImage.Url = urlBox.Text;
            testImage1.Url = urlBox.Text;
        }

        private void testEBTNN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void testProChangeBtn_Click(object sender, RoutedEventArgs e)
        {


        }

        private void testProModeBtn_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}