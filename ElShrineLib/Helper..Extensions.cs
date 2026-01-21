using ElShrine.Common.DataStructure;
using System.Globalization;
using System.Text;

namespace ElShrine
{
    public static class Extensions
    {
        //The extension methods, which are grouped by the main type, are defined here.
        //The helper methods(members, props and more) are classified with catalogy like 'string.helper'.
        #region string
        public static string BuildString<T>(this IEnumerable<T> values, Func<T, string>? toString = null, string? split = null, bool endSplit = false, StringBuilder? useStrBuilder = null)
        {
            useStrBuilder ??= new StringBuilder();
            split ??= ", ";
            toString ??= static (o) => o?.ToString() ?? "null";
            foreach (var value in values)
            {
                useStrBuilder.Append(toString.Invoke(value));
                var last = values.Last();
                var equalLast = (last is not null && value is not null && last.Equals(value)) || (last is null && value is null);
                if (endSplit || !equalLast) useStrBuilder.Append(split);
            }
            return useStrBuilder.ToString();
        }
        public static string GetPural(this string str, int num)
            => str.GetPural(int.Abs(num) > 1);
        public static string GetPural(this string str, bool isPural)
            => !isPural ? str :
            str.EndsWith('s') || str.EndsWith('x') || str.EndsWith('z') ||
            str.EndsWith("sh") || str.EndsWith("ch") ? str + "es" :
            str.EndsWith('y') && str.Length > 1 && !"aeiou".Contains(str[^2]) ?
            str[..^1] + "ies" : str + "s";
        public static string GetPuralWithNum(this string str, int num)
            => $"{num} {str.GetPural(num)}";
        #region Ignore case methods
        private const StringComparison IgnoreCaseType = StringComparison.OrdinalIgnoreCase;
        public static bool EqualIgnoreCase(this string? str, string? other)
            => (str is null && other is null) || (str is not null && other is not null && str.Equals(other, IgnoreCaseType));
        public static int IndexOfIngoreCase(this string str, char target)
            => str.IndexOf(target, IgnoreCaseType);
        public static bool ContainsIgnoreCase(this string str, string pattern)
            => str.Contains(pattern, IgnoreCaseType);
        public static bool TryReplaceIgnoreCase(this string str, string remove, out string newStr)
        {
            var result = str.ContainsIgnoreCase(remove);
            if (result) newStr = str.Replace(remove, string.Empty, IgnoreCaseType);
            else newStr = str;
            return result;
        }
        #endregion
        public static int GetDeletionDistance(this string target, string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return target?.Length ?? 0;
            pattern = pattern.ToUpperInvariant();
            target = target.ToUpperInvariant();
            int patternIndex = 0;
            foreach (char c in target)
            {
                if (patternIndex < pattern.Length && c == pattern[patternIndex]) patternIndex++;
            }
            return target.Length - patternIndex;
        }
        public static bool IsSubSequenceOf(this string sub, string parent, bool sameHead = true)
        {
            if (string.IsNullOrWhiteSpace(sub)) return true;
            if (string.IsNullOrWhiteSpace(parent)) return false;
            string subLower = sub.ToLowerInvariant(), parentLower = parent.ToLowerInvariant();
            int subIndex = 0, parentIndex = 0;
            if (sameHead)
            {
                if (subLower[0] != parentLower[0]) return false;
                subIndex = 1;
                parentIndex = 1;
            }
            while (subIndex < subLower.Length && parentIndex < parentLower.Length)
            {
                if (subLower[subIndex] == parentLower[parentIndex]) subIndex++;
                parentIndex++;
            }
            return subIndex == subLower.Length;
        }
        public static string ToTitle(this string str, bool capitalizeAllWords = false)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            if (capitalizeAllWords)
            {
                TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
                return textInfo.ToTitleCase(str.ToLower());
            }
            else return char.ToUpper(str[0]) + str[1..];
        }
        #region string.helper

        #endregion
        #endregion

        #region cloneable
        public static T Clone<T>(this T obj) where T : ICloneable<T>
        {
            var clone = (obj as ICloneable).Clone();
            if (clone is T t) return t;
            else throw new InvalidCastException($"The Clone method gives a instance(type:{clone.GetType()}) can not convert to type {typeof(T)}");
        }
        #endregion
    }
}
