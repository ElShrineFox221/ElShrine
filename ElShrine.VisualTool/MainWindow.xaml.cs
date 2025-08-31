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
                var mainWindow = EWindowViewModelBase.SetWindowViewModel<MainWindowViewModel>(this);
                mainWindow.ModuleManager.TabsController = ModulesTabControl;
                moduleManager = mainWindow.ModuleManager;
                DebugConsoleProgram.Initilize();
                ConsoleManager.SetController(ConsoleVM.GetInstance());
            };
        }
        private ModuleManagerVM? moduleManager = null;
        private void EnabledModulesListView_DragItemDropped(object sender, object item, ListView source)
        {
            if(item is ModuleInfoVM mvm)
            {
                mvm.Enabled = true;
                moduleManager?.RefreshIndexes();
            }
        }

        private void DisabledModulesListView_DragItemDropped(object sender, object item, ListView source)
        {
            if (item is ModuleInfoVM mvm)
            {
                mvm.Enabled = false;
                moduleManager?.RefreshIndexes();
            }
        }
    }
}