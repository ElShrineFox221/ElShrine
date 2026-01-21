namespace ElShrine.Common.Interpreter
{
    public enum StatementPatternType
    {
        Repeat, // repeat children sequence 0 or more times
        Required, // equivalent repeat 1 time
        Optional, // equivalent repeat 0 or 1 time
        Or, // any of children sequence matched, only the first matched one will be used
        SingleToken, // single token
        Expression, // invoke the expression parser, it will match greedily
        Block, // a block of statements, tokens in block will be packaged into a block token
    }
}
