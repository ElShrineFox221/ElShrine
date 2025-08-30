using System.Text;

namespace ElShrine
{
    public static class CommonHelper
    {
        #region Array
        public static T[] Slice<T>(this IEnumerable<T> values, int startIndex, int endIndex)
            => [.. values.Skip(startIndex).Take(endIndex - startIndex + 1)];

        public static void ReplaceAll<T>(this IList<T> list, IEnumerable<T> values)
        {
            list.Clear();
            if (list is List<T> sysList) sysList.AddRange(values);
            else
            {
                foreach (var value in values) list.Add(value);
            }
        }
        public static T GetOrCreate<T>(this IList<T> list, int index, Func<T> instantiate, bool modifySource = false)
        {
            T result;
            if (index >= list.Count || index < 0) result = instantiate();
            else result = list[index];
            if (modifySource) list.Add(result);
            return result;
        }
        public static T Clamped<T>(this IList<T> list, int index)
        {
            var clampedIndex = Math.Min(Math.Max(index, 0), list.Count - 1);
            return list[clampedIndex];
        }
        #endregion

        #region For
        [Obsolete("Donot use this while the mount came huge.", false)]
        public static void Efor<T>(Action<int> body, IEnumerable<T> values, int indexToFirst = 0, int indexToLast = 0, bool reverse = false)
        {
            Efor(body, values.Count(), indexToFirst, indexToLast, reverse);
        }
        public static void Efor(Action<int> body, int values, int indexToFirst = 0, int indexToLast = 0, bool reverse = false)
        {
            if (reverse) for (int i = values - indexToLast - 1; i > indexToFirst; i--) body.Invoke(i);
            else for (int i = indexToFirst; i < values - indexToLast; i++) body.Invoke(i);
        }
        public static void Efor<T>(Action<int, int> body, IEnumerable<T> values0, IEnumerable<T> values1, int indexToFirst0 = 0, int indexToLast0 = 0, int indexToFirst1 = 0, int indexToLast1 = 0, bool reverse = false) =>
            Efor(body, values0.Count(), values1.Count(), indexToFirst0, indexToLast0, indexToFirst1, indexToLast1, reverse);
        public static void Efor(Action<int, int> body, int values0, int values1, int indexToFirst0 = 0, int indexToLast0 = 0, int indexToFirst1 = 0, int indexToLast1 = 0, bool reverse = false)
        {
            if (reverse) for (int i = values0 - indexToLast0 - 1; i > indexToFirst0; i--) for (int j = values1 - indexToLast1 - 1; j > indexToFirst1; j--) body.Invoke(i, j);
            else for (int i = indexToFirst0; i < values0 - indexToLast0; i++) for (int j = indexToFirst1; j < values1 - indexToLast1; j++) body.Invoke(i, j);
        }
        #endregion

        #region String
        public const char SPACE = ' ';
        public const char COMMA = ',';
        public const char DOT = '.';

        private const StringComparison IgnoreCaseType = StringComparison.OrdinalIgnoreCase;
        public static bool IsEmpty(this string str) => str == Const.EmptyStr;
        public static bool IsNotEmpty(this string str) => str != Const.EmptyStr;

        public static bool EqualIgnoreCase(this string str, string other) => str.Equals(other, IgnoreCaseType);
        public static bool NullableEqualIgnoreCase(this string? str, string? other) =>
            (str is null && other is null) || (str is not null && other is not null && str.Equals(other, IgnoreCaseType));
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

        public static string BuildString<T>(this IEnumerable<T> values, Func<T, string>? toString = null, string? split = null, bool endSplit = false, StringBuilder? useStrBuilder = null)
        {
            useStrBuilder ??= new StringBuilder();
            split ??= ", ";
            toString ??= (o) => o?.ToString() ?? "null";
            foreach (var value in values)
            {
                useStrBuilder.Append(toString.Invoke(value));
                var last = values.Last();
                var equalLast = (last is not null && value is not null && last.Equals(value)) || (last is null && value is null);
                if (endSplit || !equalLast) useStrBuilder.Append(split);
            }
            return useStrBuilder.ToString();
        }
        public static string ArrowIntent(int num) => new('>', num);
        public static string GetPural(this string str, int num)
            => str.GetPural(int.Abs(num) > 1);
        public static string GetPural(this string str, bool isPural) 
            => !isPural ? str :
            str.EndsWith('s') || str.EndsWith('x') || str.EndsWith('z') ||
            str.EndsWith("sh") || str.EndsWith("ch") ? str + "es" :
            str.EndsWith('y') && str.Length > 1 && !"aeiou".Contains(str[^2]) ?
            str[..^1] + "ies" : str + "s";

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
            if (sub.IsEmpty()) return true;
            if (parent.IsEmpty()) return false;
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
        #endregion

        #region Numeric
        public static double StandardEase(double x)
            => (Math.Sin((x - 0.5f) * Math.PI) + 1f) / 2f;
        //double.Pow(x, 5) * 6 - double.Pow(x, 4) * 15 + double.Pow(x, 3) * 10;
        public static double SinEaseLerp(double from, double to, double rate)
        {
            var sub = to - from;
            var result = StandardEase(rate) * sub + from;
            return result;
        }
        public static double Shift(this double value, double offset, double max, double min)
        {
            if (max < min) throw new("Max should be bigger than min.");
            var r = value + offset; var sub = max - min;
            while (r > max) r -= sub;
            while (r < min) r += sub;
            return r;
        }
        public static float Shift(this float value, float offset, float max, float min)
        {
            if (max < min) throw new("Max should be bigger than min.");
            var r = value + offset; var sub = max - min;
            while (r > max) r -= sub;
            while (r < min) r += sub;
            return r;
        }
        public static int Shift(this int value, int offset, int max, int min)
        {
            if (max < min) throw new("Max should be bigger than min.");
            var r = value + offset; var sub = max - min;
            while (r > max) r -= sub;
            while (r < min) r += sub;
            return r;
        }
        public static byte Shift(this byte value, int offset, byte max = byte.MaxValue, byte min = byte.MinValue)
        {
            if (max < min) throw new("Max should be bigger than min.");
            var r = value + offset; var sub = max - min;
            while (r > max) r -= sub;
            while (r < min) r += sub;
            return (byte)r;
        }
        public static double Lerp(this double from, double to, double rate)
            => from + (to - from) * rate;
        public static float Lerp(this float from, float to, double rate)
            => (float)(from + (to - from) * rate);
        public static int Lerp(this int from, int to, double rate)
            => (int)(from + (to - from) * rate);
        public static byte Lerp(this byte from, byte to, double rate)
        {
            var lerp = (int)(from + rate * (to - from));
            lerp = lerp.Shift(0, byte.MaxValue, byte.MinValue);
            return (byte)lerp;
        }
        public static double ShiftLerp(this double from, double to, double rate, double max, double min)
            => from.Lerp(to, rate).Shift(0, max, min);
        public static byte ToByte(this double d)
           => byte.Parse(Math.Round(d, 0).ToString());
        public static int ToInt32(this (byte b0, byte b1, byte b2, byte b3) bytes) 
            => (bytes.b0 << 24) | (bytes.b1 << 16) | (bytes.b2 << 8) | bytes.b3;
        #endregion
    }
}
