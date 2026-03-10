using ElShrine.Modules.Log;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace ElShrine.Modules;

#region ParamParserBase
public abstract class ParamParserBase
{
    public abstract Type TargetType { get; }
    public virtual int Priority => 0;
    protected abstract bool TryParse(string input, out object? result);
    public bool TryDoParse(string input, out object? result)
    {
        var suc = false;
        result = null;
        try
        {
            suc = TryParse(input, out result);
        }
        catch { }
        return suc;
    }
}
public abstract class ParamParser<T> : ParamParserBase
{
    public sealed override Type TargetType => typeof(T);
    protected sealed override bool TryParse(string input, out object? result)
    {
        if (TryParse(input, out T? typedResult))
        {
            result = typedResult;
            return true;
        }
        result = default;
        return false;
    }
    protected abstract bool TryParse(string input, out T? result);
}
#endregion

public sealed class ParamParserManager
{
    private readonly ClassesManager _cm;
    private readonly ILogger _logger;
    public ParamParserManager(ClassesManager cm, ILogManager log)
    {
        _cm = cm;
        _logger = log.Main;

        using var _ = _logger.OpenScope("Initializing param parsers manager...");
        var sw = Stopwatch.StartNew();
        //Register a recalculation delegate
        _cm.AssembliesUpdated += Recollect;
        //Instant recalculate
        Recollect([.. _cm.Assemblies]);
        _logger.ConfigEnd($"Param parsers manager initialized, {sw.GetStopwatchElapsed()}");
    }

    private readonly Dictionary<Type, ParamParserBase> paramParsers = [];
    private void Recollect(Assembly[] range)
    {
        using var _ = _logger.OpenScope("Recollecting param parsers...");
        var implements = _cm.GetImplements(typeof(ParamParserBase), range)
            .Where(t => !t.IsAbstract && t.IsClass).ToList();
        _logger.Log($"{GetParamParserText(implements.Count)} found.");
        foreach (var type in implements)
        {
            try
            {
                if (Activator.CreateInstance(type) is ParamParserBase parser)
                {
                    var key =parser.TargetType;
                    if (paramParsers.TryGetValue(key, out var exist) && exist.Priority >= parser.Priority)
                    {
                        var e = new ParamParserException($"{key}: {parser.GetType().FullName} is ignored because {exist.GetType().FullName} has higher priority.");
                        _logger.Warning(e);
                    }
                    else paramParsers[key] = parser;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
        _logger.Log($"Recollected {GetParamParserText(paramParsers.Count)}.");
        _logger.ConfigEnd($"Recollected option items.");
    }
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
        if (paramParsers.TryGetValue(targetType, out var parser)) return parser.TryDoParse(input, out result);
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
                var tokens = TokenizeArray(input);
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

    private const char SPACE = ' ';
    public static List<string> Tokenize(string str, List<(char left, char right)> bracketSets, char split, bool removeOuterBracket)
    {
        List<string> tokens = [];
        StringBuilder? tokenBuilder = null;
        bool splitEqualSpace = split == SPACE, lastInToken = false;
        char? currentLeft = null, currentRight = null;

        for (int index = 0, cachedSpaceCount = 0, braketDegree = 0; index < str.Length; index++)
        {
            char currentChar = str[index];
            if (braketDegree == 0)
            {

                if (splitEqualSpace)
                {
                    if (currentChar == SPACE)
                    {
                        if (lastInToken) finishBuildToken();
                        else continue;
                    }
                    else
                    {
                        var bracketSetIndexByLeft = bracketSets.FindIndex(cs => cs.left == currentChar);
                        if (bracketSetIndexByLeft != -1)
                        {
                            braketDegree++;
                            currentLeft = bracketSets[bracketSetIndexByLeft].left;
                            currentRight = bracketSets[bracketSetIndexByLeft].right;
                        }
                        else if (bracketSets.FindIndex(cs => cs.right == currentChar) != -1) throw new("Meet a right bracket <> but no matched left bracket.");
                        if (!removeOuterBracket || bracketSetIndexByLeft == -1) appendChar(currentChar);
                        lastInToken = true;
                    }
                }
                else
                {
                    //not space split
                    if (currentChar == SPACE)
                    {
                        if (lastInToken) cachedSpaceCount++;
                        else cachedSpaceCount = 0;
                    }
                    else if (currentChar == split) finishBuildToken();
                    else
                    {
                        while (cachedSpaceCount > 0)
                        {
                            appendChar(SPACE);
                            cachedSpaceCount--;
                        }
                        var bracketSetIndexByLeft = bracketSets.FindIndex(cs => cs.left == currentChar);
                        if (bracketSetIndexByLeft != -1)
                        {
                            braketDegree++;
                            currentLeft = bracketSets[bracketSetIndexByLeft].left;
                            currentRight = bracketSets[bracketSetIndexByLeft].right;
                        }
                        else if (bracketSets.FindIndex(cs => cs.right == currentChar) != -1) throw new("Meet a right bracket <> but no matched left bracket.");
                        if (!removeOuterBracket || bracketSetIndexByLeft == -1) appendChar(currentChar);
                        lastInToken = true;
                    }
                }
            }
            else
            {
                tokenBuilder ??= new();
                if (currentChar == currentRight) braketDegree--;
                else if (currentChar == currentLeft) braketDegree++;
                if (!removeOuterBracket || braketDegree != 0 || currentChar != currentLeft && currentChar != currentRight) appendChar(currentChar);
                lastInToken = true;
            }
        }

        finishBuildToken();
        return tokens;
        void appendChar(char c) => (tokenBuilder ??= new()).Append(c);
        void finishBuildToken()
        {
            if (tokenBuilder is not null)
            {
                tokens.Add(tokenBuilder.ToString());
                tokenBuilder = new();
                lastInToken = false;
            }
            else tokenBuilder = new();
        }
    }
    public static List<string> TokenizeCommand(string cmdStr)
    {
        var bracketSets = new List<(char left, char right)>
        {
            ('\"', '\"'),
            ('(', ')'),
            ('[', ']'),
        };
        return Tokenize(cmdStr, bracketSets, ' ', removeOuterBracket: true);
    }
    public static List<string> TokenizeArray(string str)
    {
        var bracketSets = new List<(char left, char right)>
        {
            ('\"', '\"'),
            ('(', ')'),
            ('[', ']'),
        };
        return Tokenize(str, bracketSets, ',', removeOuterBracket: false);
    }
}
