namespace ElShrine.Common.Interpreter
{
    public interface ITokenizer
    {
        ITokenRegistry Registry { get; }
        abstract static ITokenizer CreateTokenizer(ITokenRegistry registry);
        IReadOnlyList<IToken> Tokenize(string source);
    }
}
