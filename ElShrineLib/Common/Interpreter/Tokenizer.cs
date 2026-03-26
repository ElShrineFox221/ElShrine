namespace ElShrine.Common.Interpreter
{
    public sealed class Tokenizer : ITokenizer
    {
        private Tokenizer(ITokenRegistry registry) => Registry = registry;
        public ITokenRegistry Registry { get; init; }

        public static ITokenizer CreateTokenizer(ITokenRegistry registry) => new Tokenizer(registry);

        public IReadOnlyList<IToken> Tokenize(string source)
        {
            var tokens = new List<IToken>();
            var input = source.AsSpan();
            int currentPos = 0;
            while (currentPos < source.Length)
            {
                if (input.IsEmpty) break;
                var c = input[0];
                if(char.IsWhiteSpace(c))
                {
                    input = input[1..];
                    currentPos++;
                    continue;
                }
                
                //
                IToken? foundToken = null;
                var nextPos = 0;
                foreach (var IToken in Registry.ProtoTokens.Values)
                {
                    var r = IToken.ParseNextPos(input);
                    if (r > nextPos)
                    {
                        nextPos = r;
                        foundToken = IToken;
                    }
                }
                if (nextPos > 0 && foundToken is not null)
                {
                    var span = input[..nextPos];
                    input = input[nextPos..];
                    currentPos += nextPos;
                    tokens.Add(foundToken.GetToken(span));
                }
                else throw new InvalidOperationException($"Unrecognized para at index {currentPos}, para:\"{input}\"");
            }
            return tokens;
        }
    }
}
