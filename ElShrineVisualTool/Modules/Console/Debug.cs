using ElShrine;
using ElShrine.ECommand;
using ElShrine.EConsole;

namespace ElShrine.Modules.Console
{
    [CommandCarrier(Name = Const.EmptyStr)]
    public static class Debug
    {
        public static void GachaSimulateTest(int count, int repeat)
        {
            double _percent = 1 / 100d;
            double _6s = 2 * _percent, _5s = 10 * _percent, _4s = 60 * _percent, _factor = 40 * _percent;
            Random rand = new(DateTime.Now.Microsecond);
            for (int i0 = 0; i0 < repeat; i0++)
            {
                int not6count = 0;
                int _6c = 0, _6ch1 = 0, _6ch2 = 0, _6ch3 = 0, _6cm = 0, _5c = 0, _4c = 0, _3c = 0;
                for (int i = 0; i < count; i++)
                {
                    double r = rand.NextDouble();
                    if (r <= _6s + Math.Max(0, (not6count - 49) * _factor))
                    {
                        not6count = 0;
                        _6c++;
                        if (rand.NextDouble() > 0.3)
                        {
                            if (rand.NextDouble() > 0.5) _6ch1++;
                            else _6ch2++;
                        }
                        else if (rand.NextDouble() > 0.66) _6ch3++;
                        else _6cm++;
                    }
                    else
                    {
                        not6count++;
                        if (r <= _5s) _5c++;
                        else if (r <= _4s) _4c++;
                        else _3c++;
                    }
                }
                ConsoleManager.ListInfo(new($"6={_6c}, hit1={_6ch1}, hit2={_6ch2}, hit3={_6ch3}, mis={_6cm}, 5={_5c}, 4={_4c}, 3={_3c}") { IgnoreTime = true });
            }
        }
    }
}
