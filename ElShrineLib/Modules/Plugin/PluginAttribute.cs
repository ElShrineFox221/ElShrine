namespace ElShrine.Modules.Plugin;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PluginAttribute : ValidatableClassAttribute
{
    public string Name = string.Empty;
    public string Author = string.Empty;
    public string Version = string.Empty;
    public string Description = string.Empty;
    protected override bool ValidateType(Type typeToValidate)
    {
        var hasImplement = typeToValidate.IsImplementOf(typeof(IPlugin));
        if (!hasImplement)
            ValidateFailedReason = $"{typeToValidate.FullName} is not implement of {nameof(IPlugin)}.";
        return hasImplement;
    }
}
