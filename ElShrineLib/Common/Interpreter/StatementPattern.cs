using ElShrine.Common.DataStructure;

namespace ElShrine.Common.Interpreter
{
    public class StatementPattern
    {
        public StatementPatternType Type { get; init; }
        public string TokenName { get; init; } = string.Empty;
        public string NodeName { get; set; } = string.Empty;
        public List<StatementPattern> Children { get; init; } = [];
        public int MinCount { get; set; }
        public int MaxCount { get; set; }
        public ASTStatementBuilder? StatementBuilder { get; set; }

        private StatementPattern(StatementPatternType type, params StatementPattern[] patterns)
        {
            Type = type;
            Children = [.. patterns];
        }

        public static implicit operator StatementPattern(string tokenRegName) => SingleToken(tokenRegName);

        #region Build
        public static StatementPattern Required(params StatementPattern[] patterns)
            => new StatementPattern(StatementPatternType.Required, patterns).Range(1, 1);
        public static StatementPattern Optional(params StatementPattern[] patterns)
            => new StatementPattern(StatementPatternType.Optional, patterns).Range(1, 0);
        public static StatementPattern Repeat(params StatementPattern[] patterns)
            => new(StatementPatternType.Required, patterns);
        public static StatementPattern Or(params StatementPattern[] patterns)
            => new StatementPattern(StatementPatternType.Or, patterns).Range(1, 1);
        public static StatementPattern SingleToken(string tokenRegName)
            => new(StatementPatternType.SingleToken)
            {
                TokenName = tokenRegName,
            };
        public static StatementPattern Block()
            => new(StatementPatternType.Block) { TokenName = TokenRegistry.TOKEN_BLOCK.RegName };
        public static StatementPattern Expression()
            => new(StatementPatternType.Expression);
        #endregion

        #region Set props
        public StatementPattern Name(string name)
        {
            NodeName = name;
            return this;
        }
        public StatementPattern Range(int maxCount = -1, int minCount = -1)
        {
            MaxCount = maxCount;
            MinCount = minCount;
            return this;
        }
        public StatementPattern Builder(ASTStatementBuilder builder)
        {
            StatementBuilder = builder;
            return this;
        }
        #endregion

        public bool TryMatch(Interpreter interpreter, ValueStream<IToken> tokens, out ASTNode? node)
        {
            if (InnerTryMatch(interpreter, tokens, out var results))
            {
                var rootResult = new MatchResult();
                foreach (var res in results) rootResult.Merge(res, string.Empty);
                node = StatementBuilder is not null ? StatementBuilder(rootResult) : rootResult.AsNode();
                return true;
            }
            node = null;
            return false;
        }
        private bool InnerTryMatch(Interpreter interpreter, ValueStream<IToken> tokens, out List<MatchResult> results)
        {
            results = [];
            int initialIndex = tokens.CurrentIndex;
            int matchedCount = 0;
            bool suc = true;
            var singleResult = new MatchResult();
            switch (Type)
            {
                case StatementPatternType.Repeat:
                case StatementPatternType.Required:
                case StatementPatternType.Optional:
                    while (true)
                    {
                        var cachedIndex = tokens.CurrentIndex;
                        var iterationResult = new MatchResult();
                        bool localSuc = true;
                        foreach (var child in Children)
                        {
                            if (!child.InnerTryMatch(interpreter, tokens, out var childMatch))
                            {
                                localSuc = false;
                                tokens.CurrentIndex = cachedIndex;
                                break;
                            }
                            foreach (var match in childMatch) iterationResult.Merge(match, child.NodeName);
                        }

                        if (localSuc)
                        {
                            matchedCount++;
                            results.Add(iterationResult);
                        }
                        else break;

                        if (MaxCount > 0 && matchedCount >= MaxCount) break;
                        if (tokens.CurrentIndex == cachedIndex) break;
                    }
                    suc = matchedCount >= MinCount;
                    break;

                case StatementPatternType.Or:
                    suc = false;
                    foreach (var child in Children)
                    {
                        if (child.InnerTryMatch(interpreter, tokens, out var childMatch))
                        {
                            foreach (var match in childMatch) singleResult.Merge(match, child.NodeName);
                            results.Add(singleResult);
                            suc = true;
                            break;
                        }
                    }
                    break;
                case StatementPatternType.Expression:
                    if (interpreter.PrattParser is not null)
                    {
                        var exprNode = interpreter.PrattParser.Parse(tokens);
                        if (exprNode is not null)
                        {
                            singleResult.ParsedNode = exprNode;
                            results.Add(singleResult);
                            suc = true;
                        }
                        else suc = false;
                    }
                    else suc = false;
                    break;
                case StatementPatternType.Block:
                    if (tokens.Peek().RegName == TokenName && tokens.Peek() is TOKEN_BLOCK t_block)
                    {
                        var innerTokens = new ValueStream<IToken>([.. t_block.InnerTokens], TokenRegistry.TOKEN_END);
                        var blockNode = interpreter.StatementParser.Parse(interpreter, innerTokens);
                        tokens.Consume();
                        if (blockNode is not null)
                        {
                            singleResult.ParsedNode = blockNode;
                            results.Add(singleResult);
                        }
                        suc = true;
                    }
                    else suc = false;
                    break;
                case StatementPatternType.SingleToken:
                    if (tokens.Peek().RegName == TokenName)
                    {
                        tokens.Consume();
                        suc = true;
                    }
                    else suc = false;
                    break;
            }

            if (!suc)
            {
                tokens.CurrentIndex = initialIndex;
                results.Clear();
            }
            return suc;
        }
    }
}
