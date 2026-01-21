using System.Globalization;

namespace ElShrine
{
    public static class CommonHelper
    {
        #region Array
        public static T[] Slice<T>(this IEnumerable<T> values, int startIndex, int endIndex)
            => [.. values.Skip(startIndex).Take(endIndex - startIndex + 1)];

        public static void ReplaceAll<T>(this IList<T> list, params IEnumerable<T> values)
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

        #region String
        public const char SPACE = ' ';
        public const char COMMA = ',';
        public const char DOT = '.';

        
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
