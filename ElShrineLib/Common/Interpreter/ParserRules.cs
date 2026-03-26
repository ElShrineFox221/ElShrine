namespace ElShrine.Common.Interpreter
{
    public sealed record PrattParserRule(int RBP, NudParse? Nud, LedParse? Led);
    public sealed record StatementParserRule(StatementPattern Pattern);
}
