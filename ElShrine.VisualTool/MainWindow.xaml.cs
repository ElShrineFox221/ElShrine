using ElShrine.Debug;
using ElShrine.EConsole;
using ElShrine.VisualTool.Modules.Console.ViewModel;
using ElShrine.Wpf.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace ElShrine.VisualTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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
                
                ConsoleManager.SetController(ConsoleVM.GetInstance());
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
    }
}