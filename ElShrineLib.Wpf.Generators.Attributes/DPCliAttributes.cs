using System;

namespace ElShrine.Wpf;

[AttributeUsage(AttributeTargets.Interface)]
public sealed class GenerateDPCliDeclaresAttribute : Attribute
{
    public Type? DefaultDPOwnerType { get; set; } = null;
    public bool AutoGenerate { get; set; } = true;
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateDPCliAttribute(params string[] ignoreProps) : Attribute
{
    public Type? DPOwnerType { get; set; } = null;
    public string[] IgnoreProps { get; set; } = ignoreProps;
}