namespace ElShrine.Common.Interpreter
{
    public sealed class IfStatementNode : StatementNode
    { 
        private sealed class SingleIfStatementNode(ASTNode? cond, ASTNode statement) : StatementNode(2)
        {
            public bool evaluateResult = false;
            public override IASTResult Evaluate(IASTContext context)
            {
                evaluateResult = true;
                var r = cond?.Evaluate(context);
                if(r is null || r.Value is true) return statement.Evaluate(context);
                evaluateResult = false;
                if (r.Error is not null) return r;
                return new ASTResult(null, this, null);
            }
        }
        protected override int MinimumChildrenCount => 2;

        public const string IF_COND = nameof(IF_COND);
        public const string IF_STATEMENT = nameof(IF_STATEMENT);
        public const string ELSE_IF = nameof(ELSE_IF);
        public const string ELSE_IF_COND = nameof(ELSE_IF_COND);
        public const string ELSE_IF_STATEMENT = nameof(ELSE_IF_STATEMENT);
        public const string ELSE = nameof(ELSE);
        public const string ELSE_STATEMENT = nameof(ELSE_STATEMENT);

        public readonly static StatementPattern IfPattern = StatementPattern.Required(
            TokenRegistry.TOKEN_IF.RegName,
            StatementPattern.Expression().Name(IF_COND),
            StatementPattern.Or(StatementPattern.Expression(), StatementPattern.Block()).Name(IF_STATEMENT),
            StatementPattern.Repeat(
                TokenRegistry.TOKEN_IF_ELSE.RegName,
                TokenRegistry.TOKEN_IF.RegName,
                StatementPattern.Expression().Name(ELSE_IF_COND),
                StatementPattern.Or(StatementPattern.Expression(), StatementPattern.Block()).Name(ELSE_IF_STATEMENT)
            ).Name(ELSE_IF),
            StatementPattern.Optional(
                TokenRegistry.TOKEN_IF_ELSE.RegName,
                StatementPattern.Or(StatementPattern.Expression(), StatementPattern.Block()).Name(ELSE_STATEMENT)
            ).Name(ELSE)
        )
        .Builder(result =>
        {
            var node = new IfStatementNode() as StatementNode;
            //if
            MatchResult? ifCond = result[IF_COND], ifState = result[IF_STATEMENT];
            node.Append(new SingleIfStatementNode(ifCond!, ifState!));
            //else if s
            var elseIfParts = result[ELSE_IF];
            foreach(var elseIfPart in elseIfParts)
            {
                MatchResult? elseIfCond = elseIfPart[ELSE_IF_COND], elseIfState = elseIfPart[ELSE_IF_STATEMENT];
                node.Append(new SingleIfStatementNode(elseIfCond!, elseIfState!));
            }
            //else
            MatchResult? elsePart = result[ELSE];
            if(elsePart is not null)
            {
                MatchResult? elseState = elsePart[ELSE_STATEMENT];
                node.Append(new SingleIfStatementNode(null, elseState!));
            }
            return node;
        });

        public override IASTResult Evaluate(IASTContext context)
        {
            object? result = null;
            foreach(var child in Children)
            {
                if(child is SingleIfStatementNode sisn)
                {
                    var r = sisn.Evaluate(context);
                    if(r.Error is not null) return r;
                    if (sisn.evaluateResult)
                    {
                        result = r.Value;
                        break;
                    }
                }
            }
            return new ASTResult(null, this, result);
        }
    }
    public sealed class ExpressionStatementNode : StatementNode
    {
        public const string EXPRESSION = nameof(EXPRESSION);

        public readonly static StatementPattern ExprPattern = StatementPattern.Required(
            StatementPattern.Expression().Name(EXPRESSION),
            StatementPattern.Optional(TokenRegistry.TOKEN_SPLIT.RegName)
        ).Builder(res =>
        {
            MatchResult? exprR = res[EXPRESSION];
            var child = exprR?.AsNode();
            var node = new ExpressionStatementNode();
            if (child != null) node.Append(child);
            return node;
        });

        public override IASTResult Evaluate(IASTContext context)
        {
            return Children[0].Evaluate(context);
        }
    }
}
