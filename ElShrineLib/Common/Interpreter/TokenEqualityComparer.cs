namespace ElShrine.Common.Interpreter
{
    public sealed class TokenEqualityComparer(IEnumerable<IToken> protoTokens) : IEqualityComparer<IToken>
    {
        public readonly IReadOnlyList<IToken> ProtoTokens = protoTokens.ToList().AsReadOnly();
        private IToken? MapToProtoToken(IToken? token)
        {
            var found = ProtoTokens.FirstOrDefault(t => t == token);
            found ??= ProtoTokens.FirstOrDefault(t => t.GetType() == token?.GetType());
            return found;
        }
        public bool Equals(IToken? x, IToken? y) => MapToProtoToken(x) == MapToProtoToken(y);
        public int GetHashCode(IToken obj) => MapToProtoToken(obj)?.GetHashCode() ?? 0;
    }
}
