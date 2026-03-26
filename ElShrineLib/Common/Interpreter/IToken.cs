namespace ElShrine.Common.Interpreter
{
    public interface IToken
    {
        string RegName { get; }
        int ParseNextPos(ReadOnlySpan<char> input);
        IToken GetToken(in ReadOnlySpan<char> content);
    }
}
