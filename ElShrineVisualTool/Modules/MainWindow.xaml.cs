using ElShrine.Debug;
using ElShrine.EConsole;
using ElShrine.Modules;
using ElShrine.Modules.Console.ViewModel;
using ElShrine.Wpf.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace ElShrine
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
                ModuleManagerVM.TabsController = ModulesTabControl;
                DebugConsoleProgram.Initilize();
                ConsoleManager.SetController(ConsoleVM.GetInstance());
            };
        }

        private void EnabledModulesListView_DragItemDropped(object sender, object item, ListView source)
        {
            if(item is ModuleInfoVM mvm)
            {
                mvm.Enabled = true;
                ModuleManagerVM.RefreshIndexes();
            }
        }

        private void DisabledModulesListView_DragItemDropped(object sender, object item, ListView source)
        {
            if (item is ModuleInfoVM mvm)
            {
                mvm.Enabled = false;
                ModuleManagerVM.RefreshIndexes();
            }
        }
    }
}