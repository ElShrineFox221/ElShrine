namespace ElShrine.ECommand
{
    public sealed class ObjectParser : ParamParserBase
    {
        public override Type ParseType => typeof(object);
        public override object? TokenTranfer(string token) => null;
        public override int Priority => CommonPriority;
    }
    public sealed class ShortParser : ParamParserBase
    {
        public override Type ParseType => typeof(short);
        public override object TokenTranfer(string token) => short.Parse(token);
        public override int Priority => CommonPriority;
    }
    public sealed class IntParser : ParamParserBase
    {
        public override Type ParseType => typeof(int);
        public override object TokenTranfer(string token) => int.Parse(token);
        public override int Priority => CommonPriority;
    }
    public sealed class LongParser : ParamParserBase
    {
        public override Type ParseType => typeof(long);
        public override object TokenTranfer(string token) => long.Parse(token);
        public override int Priority => CommonPriority;
    }
    public sealed class FloatParser : ParamParserBase
    {
        public override Type ParseType => typeof(float);
        public override object TokenTranfer(string token) => float.Parse(token); 
        public override int Priority => CommonPriority;
    }
    public sealed class DoubleParser : ParamParserBase
    {
        public override Type ParseType => typeof(double);
        public override object TokenTranfer(string token) => double.Parse(token); 
        public override int Priority => CommonPriority;
    }
    public sealed class BooleanParser : ParamParserBase
    {
        public override Type ParseType => typeof(bool);
        public override object TokenTranfer(string token) => bool.Parse(token);
        public override int Priority => CommonPriority;
    }
    public sealed class StringParser : ParamParserBase
    {
        public const char Quote = '"';
        public override Type ParseType => typeof(string);
        public override object TokenTranfer(string token) => token;
        public override int Priority => CommonPriority;
        public override (char? left, char? right) TokenBracketSet => (Quote, Quote);
    }
    public sealed class StringArrayParser : ParamParserBase
    {
        public override Type ParseType => typeof(string[]);
        public override object? TokenTranfer(string token) => TokenTranferArray<string>(this, token);
        public override int Priority => CommonPriority;
        public override (char? left, char? right) TokenBracketSet => ('[', ']');
        public override (char? split, Type innerType)? ArrayInfo => (null, typeof(string));
    }
}
