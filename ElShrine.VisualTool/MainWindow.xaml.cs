using ElShrine.Common.Interpreter;
using ElShrine.Debug;
using ElShrine.EConsole;
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
                /*const string expr = "if(a==0)x = -test(0, 2, a) + b * (c / -d)";
                var exprP = Interpreter.CreateInterpreter(new TokenRegistry(), new ParserRuleRegistry());
                var exprNode = exprP.Parse(expr);
                exprNode.Evaluate(new PTEPContext());*/
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
        
        public List<TestData> TestDatas { get; set; } = [];
        private void EListView_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 20; i++)
            {
                TestDatas.Add(new($"D{i}", i));
            }
            if(sender is ListView lv) lv.ItemsSource = TestDatas;
        }
    }public class TestData(string d, int v)
        {
            public string Data { get; set; } = d;
            public int V { get; set; } = v;
        }
}