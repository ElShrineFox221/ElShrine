using ElShrine.Common;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            ElShrine.Bootstrapper.ManualInitialize();
            ConsoleHelper.AllocConsole();
        }
    }

}
