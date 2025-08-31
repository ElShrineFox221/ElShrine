using System.Runtime.Serialization;

namespace ElShrine.VisualTool
{
    [DataContract]
    public class ModuleInfo
    {
        [DataMember] public string Name { get; set; } = "New Module";
        [DataMember] public string Version { get; set; } = "1.0";
        public string[] Tags  = [];
        public string Description = "Empty module with no description.";
        public string TemplateUri = string.Empty;
        public string TemplateName = string.Empty;
        [DataMember] public int Index { get; set; } = -1;
        [DataMember] public bool Enabled { get; set; } = false;

        public Type RootViewModelClass = typeof(object);
        public ModuleInfo Clone()
        {
            var module = this;
            var copyTags = new string[module.Tags.Length];
            module.Tags.CopyTo(copyTags, 0);
            var moduleCopy = new ModuleInfo()
            {
                Name = module.Name,
                Version = module.Version,
                Tags = copyTags,
                Description = module.Description,
                TemplateName = module.TemplateName,
                TemplateUri = module.TemplateUri,
                Index = module.Index,
                Enabled = module.Enabled,
            };
            return moduleCopy;
        }
        public bool MemberValueEqual(ModuleInfo other)
            => Name == other.Name 
            && Version == other.Version 
            && Tags.SequenceEqual(other.Tags) 
            && Description == other.Description
            && TemplateUri == other.TemplateUri
            && TemplateName == other.TemplateName
            && Index == other.Index
            && Enabled == other.Enabled;
    }
}
