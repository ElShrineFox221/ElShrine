namespace ElShrine.Common.Interpreter
{
    public class TOKEN(string regName) : IToken
    {
        public virtual string RegName => regName;
        public virtual int ParseNextPos(ReadOnlySpan<char> input) => 0;
        public virtual IToken GetToken(in ReadOnlySpan<char> content) => this;
        public override string ToString() => RegName;
    }
    public class TOKEN_MC(string regName, string Operator, StringComparison comparison = StringComparison.OrdinalIgnoreCase) : TOKEN(regName)
    {
        public readonly StringComparison Comparison = comparison;
        public override IToken GetToken(in ReadOnlySpan<char> content) => this;
        public override int ParseNextPos(ReadOnlySpan<char> input) => input.StartsWith(Operator) ? Operator.Length : 0;
    }
    public class TOKEN_OPR(string regName, string @operator) : TOKEN_MC(regName, @operator);
    public sealed class TOKEN_BLOCK(string regName, params IToken[] innerTokens) : TOKEN(regName)
    {
        public readonly IToken[] InnerTokens = innerTokens;
    }
    public sealed class TOKEN_ID(string name) : TOKEN(nameof(TOKEN_ID))
    {
        public readonly string Name = name;
        public override IToken GetToken(in ReadOnlySpan<char> content) => new TOKEN_ID(content.ToString());
        public override int ParseNextPos(ReadOnlySpan<char> input)
        {
            var pos = 0;
            var c = input[0];
            if (char.IsAsciiLetter(c) || c == '_') pos = 1;
            else return pos;
            while (pos < input.Length)
            {
                if (char.IsAsciiLetterOrDigit(input[pos]) || input[pos] == '_') pos++;
                else break;
            }
            return pos;
        }
        public override string ToString() => Name;
    }
    public sealed class TOKEN_STR(string content) : TOKEN(nameof(TOKEN_STR))
    {
        public readonly string Content = content;
        public override IToken GetToken(in ReadOnlySpan<char> content)
        {
            string strContent = content[1..^1].ToString();
            return new TOKEN_STR(strContent);
        }
        public override int ParseNextPos(ReadOnlySpan<char> input)
        {
            if (input.StartsWith(['"']))
            {
                var pos = 1;
                var transf = false;
                while (pos < input.Length)
                {
                    var chr = input[pos];
                    pos++;
                    if (transf || chr == '\\') transf = !transf;
                    else if (chr == '"') return pos;
                }
                throw new ExpressionParseException("Unterminated string literal");
            }
            return 0;
        }
        public override string ToString() => Content;
    }
    public sealed class TOKEN_NUM(double number) : TOKEN(nameof(TOKEN_NUM))
    {
        public readonly double Number = number;
        public override IToken GetToken(in ReadOnlySpan<char> content)
        {
            var str = content.ToString();
            var d = double.Parse(str);
            return new TOKEN_NUM(d);
        }

        public override int ParseNextPos(ReadOnlySpan<char> input)
        {
            if (input.StartsWith("nan", StringComparison.OrdinalIgnoreCase)) return 3;
            var c = input[0];
            var pos = 0;
            var doted = false;
            if (char.IsNumber(c)) pos = 1;
            while (pos < input.Length)
            {
                c = input[pos];
                if (char.IsNumber(c)) pos++;
                else if (!doted && c == '.')
                {
                    doted = true;
                    pos++;
                }
                else break;
            }
            return pos;
        }
    }
}
