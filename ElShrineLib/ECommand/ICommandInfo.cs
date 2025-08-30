using System.Reflection;
using static ElShrine.CommonHelper;

namespace ElShrine.ECommand
{
    public interface ICommandInfo
    {
        public string Name { get; }
        public string? OverrideName { get; }
        public string Description { get; }
        public string UsageTooltip { get; }
        public ICommandCarrierInfo CarrierInfo { get; }
        public MethodInfo MethodInfo { get; }
        public Type[] Parameters { get; }

        public List<string> Tokenize(string str, char split = COMMA, bool removeOuterBracket = true);
        public List<object?> ParseParams(List<string> tokens);
    }
}
