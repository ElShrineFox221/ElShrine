using ElShrine.EOption;
using ElShrine.Wpf;

namespace ElShrine.VisualTool
{
    public class WpfPageInfoVM(WpfPageInfo model) : ViewModelBase<WpfPageInfo>(model)
    {
        public string Name => Model.Name;
        public string Description => Model.Description;
        public string Version => Model.Version;
        public string[] Tags => Model.Tags;
        public string DataTemplateUri => Model.TemplateUri;
        public string DataTemplateName => Model.TemplateName;

        public bool Enabled
        {
            get => Model.Enabled;
            set
            {
                Model.Enabled = value;
                NotifyPropertiesChanged(nameof(Enabled));
            }
        }
        public Type RootViewModelClass => Model.RootViewModelClass;
        public ViewModelBase ViewModel
        {
            get
            {
                object? result;
                if (RootViewModelClass.IsImplementOf(typeof(IInitializable<>))) result = Bootstrapper.GetInstance(RootViewModelClass);
                else result = Activator.CreateInstance(RootViewModelClass);
                if (result is not null && result is ViewModelBase vmb) return vmb;
                else throw new($"Failed get viewmodel instance with type <{RootViewModelClass}>");
            }
        }
    }
}
