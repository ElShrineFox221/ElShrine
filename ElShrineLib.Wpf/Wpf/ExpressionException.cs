using System;

namespace ElShrine.Wpf
{
    public class ExpressionParseException(string message) : Exception($"Expression parsing failed: {message}");
}
