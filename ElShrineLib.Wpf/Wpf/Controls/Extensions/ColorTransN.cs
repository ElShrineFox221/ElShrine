using ElShrine.EGraphic;
using ElShrine.Old.Wpf.Model;
using ElShrine.Wpf.Controls.State;
using ElShrine.Wpf.Converters;
using ElShrine.Wpf.UITheme;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static ElShrine.EConsole.ConsoleManager;

namespace ElShrine.Wpf.Controls.Extensions
{
    public static class ColorTransN
    {
        #region Color Calculations
        private static double ClampFactor(double factor) => Math.Clamp(factor, 0.0, 1.0);
        /// <summary>
        /// 使颜色变深，通过减少 AHSL 空间中的 L (Lightness) 分量实现。
        /// </summary>
        /// <param name="colorData">原始颜色</param>
        /// <param name="factor">变暗系数，0.0 (不变) 到 1.0 (全黑)</param>
        private static ColorData DARKEN(this ColorData colorData, double factor)
        {
            factor = ClampFactor(factor);
            var ahslColor = colorData.ToAHSL();
            byte oldL = ahslColor.V3;
            byte newL = (byte)(oldL * (1.0 - factor));
            var newAhslColor = new ColorData(ahslColor.Data, ColorSpace.AHSL)
            {
                A = ahslColor.A,
                V1 = ahslColor.V1,
                V2 = ahslColor.V2, 
                V3 = newL
            };
            return newAhslColor.To(colorData.Space);
        }
        /// <summary>
        /// 使颜色变亮，通过增加 AHSL 空间中的 L (Lightness) 分量实现。
        /// </summary>
        /// <param name="colorData">原始颜色</param>
        /// <param name="factor">变亮系数，0.0 (不变) 到 1.0 (全白)</param>
        private static ColorData LIGHTEN(this ColorData colorData, double factor)
        {
            factor = ClampFactor(factor);
            var ahslColor = colorData.ToAHSL();
            byte oldL = ahslColor.V3;
            byte newL = (byte)(oldL + (255 - oldL) * factor);
            var newAhslColor = new ColorData(ahslColor.Data, ColorSpace.AHSL)
            {
                A = ahslColor.A,
                V1 = ahslColor.V1, 
                V2 = ahslColor.V2,
                V3 = newL 
            };
            return newAhslColor.To(colorData.Space);
        }
        /// <summary>
        /// 直接设置颜色的 Alpha (透明度) 分量。
        /// </summary>
        /// <param name="colorData">原始颜色</param>
        /// <param name="alphaFactor">Alpha 因子，0.0 (完全透明) 到 1.0 (完全不透明)</param>
        private static ColorData SET_A(this ColorData colorData, double alphaFactor)
        {
            alphaFactor = ClampFactor(alphaFactor);
            byte newAlpha = (byte)(255 * alphaFactor);
            var newColor = colorData.Clone();
            newColor.A = newAlpha;
            return newColor;
        }
        /// <summary>
        /// 颜色线性插值（LERP），用于解析表达式。
        /// </summary>
        /// <param name="from">起始颜色</param>
        /// <param name="to">目标颜色</param>
        /// <param name="rate">插值比例，0.0 (from) 到 1.0 (to)</param>
        /// <remarks>
        /// 这里调用 ColorDataExtension 中已有的 Lerp 方法，并默认使用 ARGB 空间插值。
        /// 颜色空间插值：跟随 rate 自动转换 (LerpState.Lerp)
        /// Alpha插值： Lerp (LerpState.Lerp)
        /// </remarks>
        private static ColorData LERP(this ColorData from, ColorData to, double rate)
        {
            rate = ClampFactor(rate);
            return from.Lerp(to, rate, LerpState.Lerp, LerpState.Lerp);
        }
        #endregion

        #region DP
        public static readonly DependencyProperty StateMapProperty = DependencyProperty.RegisterAttached("StateMap", typeof(string), typeof(ColorTransN), new(string.Empty, OnStateMapChanged));
        public static void SetStateMap(DependencyObject element, string value) => element.SetValue(StateMapProperty, value);
        public static string GetStateMap(DependencyObject element) => (string)element.GetValue(StateMapProperty);
        private static void OnStateMapChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not UIElement element) return;
            ControlStatusListener.Unregister(element);
            if (e.NewValue is not string expression || string.IsNullOrEmpty(expression)) stateMapConfigs.Remove(element);
            else
            {
                try
                {
                    stateMapConfigs.AddOrUpdate(element, new(expression));
                    ControlStatusListener.Register(element);
                }
                catch (Exception ex)
                {
                    stateMapConfigs.Remove(element);
                    ListErrorInfo(ex);
                }
            }
        }

        static ColorTransN()
        {
            ControlStatusListener.StatusChanged += (s, e) =>
            {
                if(s is UIElement element && stateMapConfigs.TryGetValue(element, out var configs))
                {
                    var olds = e.NewValue != e.OldValue ? GetColorStateByControlStatus(e.OldValue, configs) : [];
                    var news = GetColorStateByControlStatus(e.NewValue, configs);
                    TransHelper.GetThemeControlParent(element, out _, out var themeControl);
                    var variables = GetColorVariableValues(themeControl);
                    foreach (var state in news)
                    {
                        if (olds.Contains(state)) continue;
                        var propType = state.ColorProp;
                        var expressionTree = configs.ColorStatesReadOnly[state];
                        if (expressionTree is null) continue;
                        //
                        var resultValue = expressionTree.Evaluate(variables);
                        if (!resultValue.IsColor)
                        {
                            System.Diagnostics.Debug.WriteLine($"Expression for {propType} did not evaluate to a ColorData.");
                            continue;
                        }
                        var targetColor = resultValue.Color.ToMediaColor();
                        var targetDP = DependencyPropertyDescriptor.FromName(propType.ToString(), element.GetType(), element.GetType())?.DependencyProperty;
                        if (targetDP is not null)
                        {
                            var currentBrush = element.GetValue(targetDP) as Brush;
                            if (currentBrush is SolidColorBrush solidBrush) startColorAnimation(solidBrush, targetColor, themeControl);
                            else
                            {
                                Color startColor = ColorToSolidBrushConverter.ToColor(currentBrush).ToMediaColor();
                                var newBrush = new SolidColorBrush(startColor);
                                element.SetValue(targetDP, newBrush);
                                startColorAnimation(newBrush, targetColor, themeControl);
                            }
                        }
                    }
                    static void startColorAnimation(SolidColorBrush brush, Color targetColor, IThemeControlBase? themeControl)
                    {
                        var animation = new ColorAnimation
                        {
                            To = targetColor,
                            Duration = TimeSpan.FromSeconds(themeControl?.AnimaDurationIn ?? EnabledTrans.DefaultDurationSeconds)
                        };
                        brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                    }
                }
            };
        }
        private static Dictionary<string, ColorData> GetColorVariableValues(IThemeControlBase? themeControl)
        {
            var variables = new Dictionary<string, ColorData>(StringComparer.OrdinalIgnoreCase);
            ColorData GetBrushColor(DependencyProperty dp)
            {
                if (themeControl is DependencyObject dpo && dpo.GetValue(dp) is SolidColorBrush brush) return brush.Color.ToColorData();
                return new ColorData(0x00FFFFFF);
            }
            variables.Add("BG", GetBrushColor(ThemeProperties.BackBrushProperty));
            variables.Add("FG", GetBrushColor(ThemeProperties.FontBrushProperty));
            variables.Add("BR", GetBrushColor(ThemeProperties.PrimaryBrushProperty));
            variables.Add("SE", GetBrushColor(ThemeProperties.SecondaryBrushProperty));
            return variables;
        }
        private static IEnumerable<ColorStateKey> GetColorStateByControlStatus(ControlStatus status, StateMapConfiguration configs)
        {
            var prioStatus = status.EvaluatePrioStatus();
            var dic = new Dictionary<ColorPropertyType, ColorStateKey?>();
            foreach(var e in Enum.GetValues(typeof(ColorPropertyType)))
            {
                if (e is ColorPropertyType colorProp) dic[colorProp] = null;
            }
            bool filled = false;
            while (!filled)
            {
                var props = configs.ColorStatesReadOnly.Keys.Where(k => k.Status.EvaluatePrioStatus() == prioStatus);
                foreach (var prop in props)
                {
                    if (dic[prop.ColorProp] is null)
                    {
                        dic[prop.ColorProp] = prop;
                    }
                }
                if (prioStatus == ControlStatus.Normal || dic.All(kv => kv.Value is not null)) filled = true;
                else prioStatus = (status ^= prioStatus).EvaluatePrioStatus();
            }
            return dic.Where(kv => kv.Value.HasValue).Select(kv => kv.Value!.Value);
        }

        private readonly static ConditionalWeakTable<UIElement, StateMapConfiguration> stateMapConfigs = [];
        #endregion

        #region StateMapConfiguration
        private readonly record struct ColorStateKey(ControlStatus Status, ColorPropertyType ColorProp);
        private sealed class ColorState1
        {
            public Dictionary<ColorPropertyType, ColorExpression?> Colors = new()
            {
                { ColorPropertyType.Background, null },
                { ColorPropertyType.Foreground, null },
                { ColorPropertyType.BorderBrush, null }
            };
            public void Apply(UIElement element)
            {
                TransHelper.GetThemeControlParent(element, out _, out var themeControl);
                var variables = GetColorVariableValues(themeControl);
                foreach (var kvp in Colors)
                {
                    var propType = kvp.Key;
                    var expressionTree = kvp.Value;
                    if (expressionTree is null) continue;
                    var resultValue = expressionTree.Evaluate(variables);
                    if (!resultValue.IsColor)
                    {
                        System.Diagnostics.Debug.WriteLine($"Expression for {propType} did not evaluate to a ColorData.");
                        continue;
                    }
                    var targetColor = resultValue.Color.ToMediaColor();
                    var targetDP = DependencyPropertyDescriptor.FromName(propType.ToString(), element.GetType(), element.GetType())?.DependencyProperty;
                    if (targetDP is not null)
                    {
                        var currentBrush = element.GetValue(targetDP) as Brush;
                        if (currentBrush is SolidColorBrush solidBrush) startColorAnimation(solidBrush, targetColor, themeControl);
                        else
                        {
                            Color startColor = ColorToSolidBrushConverter.ToColor(currentBrush).ToMediaColor();
                            var newBrush = new SolidColorBrush(startColor);
                            element.SetValue(targetDP, newBrush);
                            startColorAnimation(newBrush, targetColor, themeControl);
                        }
                    }
                }
                static void startColorAnimation(SolidColorBrush brush, Color targetColor, IThemeControlBase? themeControl)
                {
                    var animation = new ColorAnimation
                    {
                        To = targetColor,
                        Duration = TimeSpan.FromSeconds(themeControl?.AnimaDurationIn ?? EnabledTrans.DefaultDurationSeconds)
                    };
                    brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                }
            }
        }
        private enum ColorPropertyType
        {
            Background,
            Foreground,
            BorderBrush
        }

        private sealed class StateMapConfiguration
        {
            private readonly Dictionary<ColorStateKey, ColorExpression> ColorStates = [];
            public IReadOnlyDictionary<ColorStateKey, ColorExpression> ColorStatesReadOnly => ColorStates.AsReadOnly();
            public static readonly HashSet<string> VariableNames = new(StringComparer.OrdinalIgnoreCase) { "BG", "FG", "BR", "SE" };
            public static readonly Dictionary<string, ColorPropertyType> PropertyNameMap = new(StringComparer.OrdinalIgnoreCase)
            {
                { "BACK", ColorPropertyType.Background },
                { "FORE", ColorPropertyType.Foreground },
                { "BORDER", ColorPropertyType.BorderBrush },
            };
            public StateMapConfiguration(string expression) => ReparseStateMap(expression);

            public void ReparseStateMap(string expression)
            {
                var config = this;
                config.ColorStates.Clear();
                var stateDefinitions = expression.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var stateDef in stateDefinitions)
                {
                    var parts = stateDef.Split('=', 2, StringSplitOptions.TrimEntries);
                    if (parts.Length != 2) continue;
                    string stateName = parts[0].Trim();
                    string propertiesString = parts[1].Trim();
                    //validate
                    if (!Enum.TryParse(typeof(ControlStatus), stateName, out var r) || r is not ControlStatus status) continue;
                    if (string.IsNullOrEmpty(propertiesString)) continue;
                    var propertyExpressions = SplitPropertyExpressions(propertiesString);
                    foreach (var propExp in propertyExpressions)
                    {
                        var propParts = propExp.Split(':', 2, StringSplitOptions.TrimEntries);
                        if (propParts.Length != 2) continue;

                        string propName = propParts[0].Trim();
                        string expString = propParts[1].Trim();
                        if (PropertyNameMap.TryGetValue(propName, out var propType))
                        {
                            var key = new ColorStateKey(status, propType);
                            var expressionTree = ParseExpression(expString, VariableNames);
                            if(expressionTree is not null) config.ColorStates[key] = expressionTree;
                        }
                    }

                }
            }
            private static IEnumerable<string> SplitPropertyExpressions(string propertiesString)
            {
                var results = new List<string>();
                int bracketCount = 0;
                int start = 0;

                for (int i = 0; i < propertiesString.Length; i++)
                {
                    char c = propertiesString[i];
                    if (c == '(') bracketCount++;
                    else if (c == ')') bracketCount--;

                    if (c == ',' && bracketCount == 0)
                    {
                        results.Add(propertiesString[start..i].Trim());
                        start = i + 1;
                    }
                }
                results.Add(propertiesString[start..].Trim());

                if (bracketCount != 0) throw new FormatException($"Mismatched parentheses in property list: {propertiesString}");
                return results.Where(s => !string.IsNullOrEmpty(s));
            }
        }
        #endregion

        #region Expression Parse

        /// <summary>
        /// 解析颜色表达式字符串并构建表达式树。
        /// </summary>
        /// <param name="expression">颜色表达式字符串 (例如: LERP(DARKEN(FG, 0.2), #WHITE, 0.1))</param>
        /// <param name="variableNames">预期的颜色变量名称集合 (用于区分变量和函数名)</param>
        /// <returns>ColorExpression 表达式树的根节点</returns>
        private static ColorExpression ParseExpression(string expression, HashSet<string> variableNames)
        {
            expression = expression.Trim();
            if (string.IsNullOrEmpty(expression)) throw new ArgumentException("Color expression cannot be empty.");
            try
            {
                return ParseRecursive(expression, variableNames);
            }
            catch (Exception ex)
            {
                throw new FormatException($"Failed to parse color expression: '{expression}'. Details: {ex.Message}", ex);
            }
        }
        private static ColorExpression ParseRecursive(string expression, HashSet<string> variableNames)
        {
            expression = expression.Trim();
            if (TryParseLiteralOrVariable(expression, variableNames, out var literalResult) && literalResult is not null) return literalResult;
            int openParenIndex = expression.IndexOf('(');
            int closeParenIndex = expression.LastIndexOf(')');
            if (openParenIndex > 0 && closeParenIndex == expression.Length - 1)
            {
                string functionName = expression[..openParenIndex].Trim();
                string argsString = expression.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1);
                var arguments = ParseArguments(argsString, variableNames);
                return new ColorFunctionExpression(functionName, arguments);
            }
            throw new FormatException($"Invalid token or structure: {expression}. Expected constant, variable, or function call.");
        }
        private static bool TryParseLiteralOrVariable(string expression, HashSet<string> variableNames, out ColorExpression? result)
        {
            expression = expression.Trim();
            string upperExpression = expression.ToUpperInvariant();
            if (variableNames.Contains(upperExpression))
            {
                result = new ColorVariableExpression(upperExpression);
                return true;
            }
            if (expression.StartsWith('#'))
            {
                string hexOrName = expression[1..].ToUpperInvariant();
                if (MediaColorHelper.NamedColors.TryGetValue(hexOrName, out var namedColor))
                {
                    result = new ColorConstantExpression(namedColor);
                    return true;
                }
                if (hexOrName.Length == 6 || hexOrName.Length == 8)
                {
                    if (int.TryParse(hexOrName, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int data))
                    {
                        if (hexOrName.Length == 6) data = unchecked((int)0xFF000000) | data;
                        result = new ColorConstantExpression(new ColorData(data, ColorSpace.ARGB));
                        return true;
                    }
                }
                throw new FormatException($"Invalid color constant format: {expression}");
            }
            if (double.TryParse(expression, NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
            {
                result = new ColorConstantExpression(number);
                return true;
            }

            result = null;
            return false;
        }
        private static List<ColorExpression> ParseArguments(string argsString, HashSet<string> variableNames)
        {
            var arguments = new List<ColorExpression>();
            int bracketCount = 0;
            int start = 0;

            for (int i = 0; i < argsString.Length; i++)
            {
                char c = argsString[i];
                if (c == '(') bracketCount++;
                else if (c == ')') bracketCount--;
                if (c == ',' && bracketCount == 0)
                {
                    string argument = argsString[start..i].Trim();
                    if (!string.IsNullOrEmpty(argument))
                    {
                        arguments.Add(ParseRecursive(argument, variableNames));
                    }
                    start = i + 1;
                }
            }
            string lastArgument = argsString[start..].Trim();
            if (!string.IsNullOrEmpty(lastArgument))
            {
                arguments.Add(ParseRecursive(lastArgument, variableNames));
            }

            if (bracketCount != 0) throw new FormatException($"Mismatched parentheses in arguments: {argsString}");
            return arguments;
        }
        /// <summary>
        /// 根据函数名执行颜色计算。
        /// </summary>
        public static ExpressionValue ExecuteFunctionFromTree(string functionName, List<ExpressionValue> args)
        {
            switch (functionName.ToUpperInvariant())
            {
                case "LERP":
                    if (args.Count != 3) throw new ArgumentException("LERP requires 3 arguments: (Color, Color, Factor)");
                    if (!args[0].IsColor || !args[1].IsColor || args[2].IsColor) throw new ArgumentException("LERP arguments type mismatch: (Color, Color, Number)");
                    return new ExpressionValue(
                        LERP(args[0].Color, args[1].Color, args[2].Number)
                    );

                case "DARKEN":
                    if (args.Count != 2) throw new ArgumentException("DARKEN requires 2 arguments: (Color, Factor)");
                    if (!args[0].IsColor || args[1].IsColor) throw new ArgumentException("DARKEN arguments type mismatch: (Color, Number)");
                    return new ExpressionValue(
                        DARKEN(args[0].Color, args[1].Number)
                    );

                case "LIGHTEN":
                    if (args.Count != 2) throw new ArgumentException("LIGHTEN requires 2 arguments: (Color, Factor)");
                    if (!args[0].IsColor || args[1].IsColor) throw new ArgumentException("LIGHTEN arguments type mismatch: (Color, Number)");
                    return new ExpressionValue(
                        LIGHTEN(args[0].Color, args[1].Number)
                    );

                case "SET_A":
                    if (args.Count != 2) throw new ArgumentException("SET_A requires 2 arguments: (Color, AlphaFactor)");
                    if (!args[0].IsColor || args[1].IsColor) throw new ArgumentException("SET_A arguments type mismatch: (Color, Number)");
                    return new ExpressionValue(
                        SET_A(args[0].Color, args[1].Number)
                    );

                default:
                    throw new NotSupportedException($"Unknown function: {functionName}");
            }
        }
        #endregion
    }
}
