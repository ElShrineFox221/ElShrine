using ElShrine.Common.DataStructure;

namespace ElShrine.Common.Interpreter
{
    public sealed class PrattParser
    {
        private PrattParser(IPrattParserRuleRegistry registry) => this.registry = registry;
        private readonly IPrattParserRuleRegistry registry;
        public ASTNode Parse(ValueStream<IToken> tokens) => ParseExpression(tokens, 0);
        public ExpressionNode ParseExpression(ValueStream<IToken> tokens, int rbp)
        {
            if (tokens.CurrentIndex >= tokens.Count) throw new ExpressionParseException("Unexpected end of tokens.");
            IToken t = tokens.Consume();
            if (!registry.Items.TryGetValue(t.RegName, out var rule)) 
                throw new ExpressionParseException($"Token '{t}' has no registered Nud or Led rule.");
            if (rule.Nud == null)
                throw new ExpressionParseException($"Token '{t}' cannot start an expression.");

            ExpressionNode left = rule.Nud(this, tokens, t);

            while (tokens.CurrentIndex < tokens.Count)
            {
                IToken nextT = tokens.Peek(); // Peek

                if (!registry.Items.TryGetValue(nextT.RegName, out var nextRule)) break;
                if (nextRule.RBP <= rbp) break;

                tokens.Consume(); // Consume
                if (nextRule.Led == null) throw new ExpressionParseException($"Token '{nextT}' has no Led rule.");

                left = nextRule.Led(this, tokens, nextT, left);
            }

            return left;
        }

        public static PrattParser CreateParser(IPrattParserRuleRegistry registry) => new(registry);
    }
}
