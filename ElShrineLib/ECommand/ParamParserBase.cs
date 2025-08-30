namespace ElShrine.ECommand
{
    public abstract class ParamParserBase
    {
        public abstract Type ParseType { get; }
        public abstract object? TokenTranfer(string token);
        public virtual int Priority => 0;
        internal const int CommonPriority = 10;

        public virtual (char? left, char? right) TokenBracketSet => (null, null);
        public virtual (char? split, Type innerType)? ArrayInfo => (null, typeof(string));
        public (char left, char right)? ValidatedBraketSet()
        {
            var (left, right) = TokenBracketSet;
            if (left.HasValue ^ right.HasValue) throw new("Bracket set should has both null or not");
            else if (left.HasValue && right.HasValue) return (left.Value, right.Value);
            else return null;
        }
        protected static T?[] TokenTranferArray<T>(ParamParserBase parser, string token)
        {
            var (split, innerType) = parser.ArrayInfo ?? throw new("Has no array info to parse.");
            var (left, right) = parser.ValidatedBraketSet() ?? ('[', ']');
            if (!(token.StartsWith(left) ^ token.EndsWith(right)))
            {
                token = token.TrimStart(left).TrimEnd(right);
                T?[] result = [];
                if (token.Length > 0)
                {
                    var innerParser = ParamParserManager.GetParameterParser(innerType);
                    var innerBracketSet = innerParser.ValidatedBraketSet() ?? throw new("Invalid bracket set.");
                    var tokens = ParamParserManager.Tokenize(token, [innerBracketSet], split ?? CommonHelper.COMMA, true);
                    result = [.. tokens.Select(t => (T?)innerParser.TokenTranfer(t))];
                }
                return result;
            }
            else throw new($"Can not parse token <{token}> as array param.");
        }
    }
}
