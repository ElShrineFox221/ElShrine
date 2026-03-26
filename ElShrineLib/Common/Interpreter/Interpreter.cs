using ElShrine.Common.DataStructure;

namespace ElShrine.Common.Interpreter
{
    public sealed class Interpreter
    {
        private Interpreter(ITokenizer tokenizer, IPrattParserRuleRegistry prattParserRuleRegistry, IStatementParserRuleRegistry statementParserRuleRegistry)
        {
            this.tokenizer = tokenizer;
            tokenRegistry = tokenizer.Registry;
            this.prattParserRuleRegistry = prattParserRuleRegistry;
            this.statementParserRuleRegistry = statementParserRuleRegistry;
            PrattParser = PrattParser.CreateParser(prattParserRuleRegistry);
            StatementParser = StatementParser.CreateParser(statementParserRuleRegistry);
        }
        public static Interpreter CreateInterpreter(ITokenizer tokenizer, IPrattParserRuleRegistry prattParserRuleRegistry, IStatementParserRuleRegistry statementParserRuleRegistry) 
            => new(tokenizer, prattParserRuleRegistry, statementParserRuleRegistry);
        public static Interpreter CreateInterpreter<TTokenizer>(ITokenRegistry? registry = null, IPrattParserRuleRegistry? prattParserRuleRegistry = null, IStatementParserRuleRegistry? statementParserRuleRegistry = null) where TTokenizer : ITokenizer
            => CreateInterpreter(TTokenizer.CreateTokenizer(registry ?? new TokenRegistry()), prattParserRuleRegistry ?? new PrattParserRuleRegistry(), statementParserRuleRegistry ?? new StatementParserRuleRegistry());
        public static Interpreter CreateInterpreter(ITokenRegistry? registry = null, IPrattParserRuleRegistry? prattParserRuleRegistry = null, IStatementParserRuleRegistry? statementParserRuleRegistry = null)
            => CreateInterpreter<Tokenizer>(registry, prattParserRuleRegistry, statementParserRuleRegistry);

        public readonly ITokenizer tokenizer;
        public readonly ITokenRegistry tokenRegistry;
        public readonly IPrattParserRuleRegistry prattParserRuleRegistry;
        public readonly IStatementParserRuleRegistry statementParserRuleRegistry;
        public StatementParser StatementParser { get; init; }
        public PrattParser PrattParser { get; init; }

        public ASTNode? Parse(string source)
        {
            var tokens = tokenizer.Tokenize(source);
            var stream = new ValueStream<IToken>([.. tokens], TokenRegistry.TOKEN_END);
            try
            {
                var node = StatementParser.Parse(this, stream);
                if (!stream.IsEnd) throw new ParseException("Failed parse tokens.");
                return node;
            }
            catch { }
            return null;
        }
        public ASTNode? ParseExpression(string source)
        {
            var tokens = tokenizer.Tokenize(source);
            var stream = new ValueStream<IToken>([.. tokens], TokenRegistry.TOKEN_END);
            try
            {
                var node = PrattParser.Parse(new([.. tokens], TokenRegistry.TOKEN_END));
                if (!stream.IsEnd) throw new ParseException("Failed parse tokens.");
                return node;
            }
            catch { }
            return null;
        }
    }
}
