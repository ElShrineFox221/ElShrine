
namespace ElShrine.Common.MathCalc.Other
{
    public class OtherCalc
    {
        public static double ExpectationCalc(double basicRate, double factor, int limit)
        {
            double total = 0, rate = basicRate;
            for (int count = 1; rate <= 1; count++)
            {
                rate = System.Math.Max(count - limit, 0) * factor + basicRate;
                double r0 = System.Math.Pow(1 - basicRate, System.Math.Min(count, limit) - 1);
                r0 *= System.Math.Min(1, rate);
                int i = count - limit;
                while (i > 0) r0 *= 1 - (i-- * factor + basicRate);
                total += r0 * count;
            }
            return total;
        }
    }
}
