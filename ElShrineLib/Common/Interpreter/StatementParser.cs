using ElShrine.Common.DataStructure;

namespace ElShrine.Common.Interpreter
{
    public sealed class StatementParser
    {
        private StatementParser(IStatementParserRuleRegistry registry) => this.registry = registry;
        private readonly IStatementParserRuleRegistry registry;
        public ASTNode Parse(Interpreter interpreter, ValueStream<IToken> tokens) => ParseStatements(interpreter, tokens);

        private StatementNode ParseStatements(Interpreter interpreter, ValueStream<IToken> tokens)
        {
            var children = new List<StatementNode>();
            foreach (var pattern in registry.Items.Values)
            {
                var suc = pattern.Pattern.TryMatch(interpreter, tokens, out var snode);
                if (suc) children.Add((snode as StatementNode)!);
            }
            if (children.Count > 1)
            {
                var node = new StatementNode();
                node.Append(children);
                return node;
            }
            else return children[0];
        }
        public static StatementParser CreateParser(IStatementParserRuleRegistry registry) => new(registry);
    }
}
