using ElShrine.Wpf.ViewModel;
using System.Windows;

namespace ElShrine.Modules
{
    public class MainWindowViewModel : EWindowViewModelBase
    {
        protected MainWindowViewModel(Window ownerWindow) : base(ownerWindow)
        {
            ModuleManagerVM.Refresh();
            Application.Current.Dispatcher.BeginInvoke(ModuleManagerVM.Reload);
        }

        public ModuleManagerVM ModuleManager { get; } = ModuleManagerVM.GetInstance();
    }
}
