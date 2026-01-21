using ElShrine.Common.DataStructure;

namespace ElShrine.Common.Interpreter
{
    public delegate ExpressionNode NudParse(PrattParser prattParser, ValueStream<IToken> tokens, IToken token);
    public delegate ExpressionNode LedParse(PrattParser prattParser, ValueStream<IToken> tokens, IToken token, ExpressionNode left);

    public delegate object? UnaryCalculator(object? value);
    public delegate object? BinaryCalculator(object? left, object? right);
    public delegate object? TernaryCalculator(object? left, object? middle, object? right);
    public delegate bool BreakCalculator(object? value);

    public delegate StatementNode ASTStatementBuilder(MatchResult matchResult);
}
