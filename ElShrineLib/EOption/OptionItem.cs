using System.Runtime.Serialization;

namespace ElShrine.EOption
{
    [DataContract]
    public class OptionItem(string name, string className, string? classOverrideName, object? value)
    {
        [DataMember] public string ItemName { get; set; } = name;
        [DataMember] public string ClassName { get; set; } = className;
        [DataMember] public string? ClassOverrideName { get; set; } = classOverrideName;
        [DataMember] public object? Value { get; set; } = value;
        [IgnoreDataMember] public string Description { get; set; } = string.Empty;
        [IgnoreDataMember] public Type ValueType { get; set; } = typeof(object);
        public bool SomeEqual(OptionItem other)
            => ItemName.EqualIgnoreCase(other.ItemName) && ClassName.EqualIgnoreCase(other.ClassName);
    }
}
