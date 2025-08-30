using ElShrine.Common.Interface;

namespace ElShrine.Old.Command
{
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public enum CommandAutoLoadMode
    {
        All, Public, Private, None
    }
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandCarrierAttribute(string name = Const.EmptyStr, bool isGlobalCarrier = false, CommandAutoLoadMode loadMode = CommandAutoLoadMode.Public) : Attribute, IEName
    {
        public string Name { get; init; } = name;
        public bool IsGlobalCarrier { get; init; } = isGlobalCarrier;
        public CommandAutoLoadMode LoadMode { get; init; } = loadMode;
    }
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute(string name = Const.EmptyStr, bool ignoreThis = false, string description = Const.EmptyStr) : Attribute, IEName
    {
        public string Name { get; init; } = name;
        public bool IgnoreThis { get; init; } = ignoreThis;
        public string Description { get; init; } = description;
    }
}
