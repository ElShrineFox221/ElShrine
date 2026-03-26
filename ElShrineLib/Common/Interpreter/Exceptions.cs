namespace ElShrine.Common.Interpreter
{
    public class ParseException(string msg) : Exception(msg);
    public class ExpressionParseException(string message) : Exception($"Expression parsing failed: {message}");
}
