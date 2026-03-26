using ElShrine.Common.Interpreter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace ElShrine.Wpf.Converters
{
    public sealed class ExpressionConverter : IValueConverter, IMultiValueConverter
    {
        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            var paramStr = parameter?.ToString();
            var temporaryResult = value;
            ASTNode? ast = null;
            if (!string.IsNullOrWhiteSpace(paramStr) && !cachedExprs.TryGetValue(paramStr, out ast)) ast = interpreter.Parse(paramStr);
            if (ast is not null) temporaryResult = ast.Evaluate(ToContext([value]));
            var result = CommonConverter.FinalizeConvert(temporaryResult, targetType);
            return result;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        public object? Convert(object?[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var paramStr = parameter?.ToString();
            var temporaryResult = values[0];
            ASTNode? ast = null;
            if (!string.IsNullOrWhiteSpace(paramStr) && !cachedExprs.TryGetValue(paramStr, out ast))
            {
                ast = interpreter.Parse(paramStr);
                if (ast is not null) cachedExprs[paramStr] = ast;
            }
            if (ast is not null)
            {
                var astResult = ast.Evaluate(ToContext(values));
                temporaryResult = astResult.Value;
            }
            var result = CommonConverter.FinalizeConvert(temporaryResult, targetType);
            return result;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #region Expression
        private readonly static Interpreter interpreter = Interpreter.CreateInterpreter();
        private readonly static Dictionary<string, ASTNode> cachedExprs = [];
        public static IASTContext ToContext(object?[] values)
        {
            //values
            Dictionary<string, object?> variables = new(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < values.Length; i++) variables.Add($"V{i}", values[i]);
            Dictionary<string, object?> constants = [];
            var methods = new Dictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(Math.Max)] = Methods.MaxMethodInfo,
                [nameof(Math.Min)] = Methods.MinMethodInfo
            };

            var context = new ASTTemporaryContext(variables, constants, methods);
            return context;
        }
        
        

        private static class Methods
        {
            private static double Max(double v0, double v1) => Math.Max(v0, v1);
            private static double Min(double v0, double v1) => Math.Min(v0, v1);
            private const BindingFlags AccessFlags = BindingFlags.Static | BindingFlags.NonPublic;
            public static readonly MethodInfo MinMethodInfo = typeof(Methods).GetMethod(nameof(Min), AccessFlags)!;
            public static readonly MethodInfo MaxMethodInfo = typeof(Methods).GetMethod(nameof(Max), AccessFlags)!;
        }
        #endregion
    }
}
