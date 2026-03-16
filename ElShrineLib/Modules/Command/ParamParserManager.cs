using ElShrine.Modules.Log;
using ElShrine.Modules.Plugin;
using System.Runtime.Loader;

namespace ElShrine.Modules.Command;

internal sealed class ParamParserManager : PluginResourceTracker<ParamParserBase>, IParamParserManager
{
    private readonly ILogger _logger;
    public ParamParserManager(IPluginManager plugin, ILogManager log) : base(plugin, false)
    {
        _logger = log.Main;

        DoCollectResources(AssemblyLoadContext.Default);
    }

    #region overrides

    protected override void DoCollectResources(AssemblyLoadContext ctx)
    {
        using var sc = OpenRecollectTextScope(_logger, ctx);
        base.DoCollectResources(ctx);
        _logger.Log($"Collected {GetParamParserText(Resources[ctx].Values.Count)}.");
    }
    #endregion

    private static string GetParamParserText(int count)
        => $"{"param parser".GetPuralWithNum(count)}";

    #region Operations
    public bool TryConvert(string input, Type targetType, out object? result)
    {
        if (targetType == typeof(string))
        {
            result = input;
            return true;
        }
        ParamParserBase? parser = null;
        if(Resources.Values.FirstOrDefault(d => d.TryGetValue(targetType, out parser)) is not null)
            return parser!.TryDoParse(input, out result);
        try
        {
            if (targetType.IsEnum)
            {
                result = Enum.Parse(targetType, input, true);
                return true;
            }
            if (targetType.IsArray)
            {
                var eleType = targetType.GetElementType()!;
                var tokens = input.TokenizeArray();
                var arr = Array.CreateInstance(eleType, tokens.Count);
                var localSuc = true;
                for (int i = 0; i < tokens.Count; i++)
                {
                    if (TryConvert(tokens[i], eleType, out var ele)) arr.SetValue(ele, i);
                    else localSuc = false;
                }
                result = arr;
                return localSuc;
            }
            result = Convert.ChangeType(input, targetType);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }
    #endregion
}
