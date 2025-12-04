using ElShrine.EGraphic;
using System.Collections.Generic;
using System.Linq;

namespace ElShrine.Wpf.Controls.Extensions
{
    public enum ExpressionNodeType
    {
        Constant,
        Variable,
        Function
    }
    public abstract class ColorExpression
    {
        public abstract ExpressionNodeType NodeType { get; }
        protected ExpressionValue? CachedResult;
        public abstract ExpressionValue Evaluate(Dictionary<string, ColorData> variables);
        public virtual void ClearCache() => CachedResult = null;
    }
    public sealed class ColorConstantExpression : ColorExpression
    {
        public override ExpressionNodeType NodeType => ExpressionNodeType.Constant;
        public ExpressionValue Value { get; }
        public ColorConstantExpression(ColorData color)
        {
            Value = new ExpressionValue(color);
            CachedResult = Value;
        }
        public ColorConstantExpression(double number)
        {
            Value = new ExpressionValue(number);
            CachedResult = Value;
        }
        public override ExpressionValue Evaluate(Dictionary<string, ColorData> variables) => Value;
    }
    public sealed class ColorVariableExpression(string variableName) : ColorExpression
    {
        public override ExpressionNodeType NodeType => ExpressionNodeType.Variable;
        public string VariableName { get; } = variableName.ToUpperInvariant();
        public override ExpressionValue Evaluate(Dictionary<string, ColorData> variables)
        {
            if (variables.TryGetValue(VariableName, out var color))
            {
                return new ExpressionValue(color);
            }
            throw new KeyNotFoundException($"Color variable '{VariableName}' not found in the current context.");
        }
    }
    public sealed class ColorFunctionExpression(string functionName, List<ColorExpression> arguments) : ColorExpression
    {
        public override ExpressionNodeType NodeType => ExpressionNodeType.Function;
        public string FunctionName { get; } = functionName.ToUpperInvariant();
        public List<ColorExpression> Arguments { get; } = arguments;
        public override ExpressionValue Evaluate(Dictionary<string, ColorData> variables)
        {
            if (CachedResult.HasValue) return CachedResult.Value;
            var evaluatedArgs = Arguments
                .Select(arg => arg.Evaluate(variables))
                .ToList();
            var result = ColorTransN.ExecuteFunctionFromTree(FunctionName, evaluatedArgs);
            CachedResult = result;
            return result;
        }
        public override void ClearCache()
        {
            CachedResult = null;
            foreach (var arg in Arguments)
            {
                arg.ClearCache();
            }
        }
    }
    public readonly struct ExpressionValue
    {
        public ColorData Color { get; }
        public double Number { get; }
        public bool IsColor { get; }
        public ExpressionValue(ColorData color) { Color = color; Number = default; IsColor = true; }
        public ExpressionValue(double number) { Color = new(); Number = number; IsColor = false; }
    }
}
