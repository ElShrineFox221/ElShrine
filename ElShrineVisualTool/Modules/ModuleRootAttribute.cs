using ElShrine.Wpf.ViewModel;

namespace ElShrine.Modules
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ModuleRootAttribute() : ValidatableAttribute
    {
        public string? Name;
        public string? Version;
        public string[] Tags = [];
        public string? Description = null;
        public string? DataTemplateUri;
        public string? DataTemplateName;
        public bool DefaultEnabled = false;
        public override bool Validate(object obj)
            => obj is Type t && t.IsImplementOf(typeof(ViewModelBase));
        public ModuleInfo ToModuleInfo(Type implement)
        {
            ModuleInfo moduleInfo = new()
            {
                Name = Name ?? implement.Name,
                Tags = Tags,
                RootViewModelClass = implement,
                Enabled = DefaultEnabled,
            }; 
            if (Version is not null) moduleInfo.Version = Version;
            if(Description is not null) moduleInfo.Description = Description;
            if(DataTemplateUri is not null) moduleInfo.TemplateUri = DataTemplateUri;
            if(DataTemplateName is not null) moduleInfo.TemplateName = DataTemplateName;
            return moduleInfo;
        }
    }
}
