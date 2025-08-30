namespace ElShrine.Wpf
{
    public record class ChangeValueConverter(int IntFactor = 1, double DoubleFactor = 1)
    {
        public bool GetIntValue(object? paramObj, out int result)
        {
            bool suc = true;
            if (paramObj is int i) result = i;
            else if (paramObj is string s && int.TryParse(s, out int i1)) result = i1;
            else
            {
                suc = false;
                result = 0;
            }
            result *= IntFactor;
            return suc;
        }
        public bool GetDoubleValue(object? paramObj, out double result)
        {
            bool suc = true;
            if (paramObj is double d) result = d;
            else if (paramObj is string s && double.TryParse(s, out double d1)) result = d1;
            else
            {
                suc = false;
                result = 0;
            }
            result *= DoubleFactor;
            return suc;
        }
    }
}
