namespace ElShrine.Modules.Command;

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
