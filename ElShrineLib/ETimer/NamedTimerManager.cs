namespace ElShrine.ETimer
{
    public static class NamedTimerManager
    {
        private readonly static List<NamedTimer> Timers = [];
        public static NamedTimer? FindTimer(Predicate<NamedTimer> predicate)
        {
            NamedTimer? result = null;
            foreach (NamedTimer timer in Timers)
            {
                if (predicate(timer))
                {
                    result = timer;
                    break;
                }
            }
            return result;
        }
        public static NamedTimer CreateTimer(int interval, bool registered = false, string? name = null)
        {
            NamedTimer timer = new(interval, registered, name ?? $"Timer{Timers.Count}");
            if (registered) Timers.Add(timer);
            return timer;
        }
        public static void DisposeETimer(NamedTimer timer)
        {
            Timers.Remove(timer);
            timer.Dispose();
        }

        public static NamedTimer SetInterval(Action action, int delay, bool registerToManager = false, string? timerRegisterName = null)
            => SetInterval(() => action.Invoke(), delay, 1, registerToManager, timerRegisterName);
        public static NamedTimer SetInterval(Action action, int interval, double times = 1000, bool registerToManager = false, string? registerName = null)
        {
            NamedTimer timer = CreateTimer(interval, registerToManager, registerName);
            timer.Elapsed += delegate
            {
                if (times-- > 0) action.Invoke();
                else timer.Dispose();
            };
            timer.Start();
            return timer;
        }
        public static NamedTimer SetInterval(Func<int, bool> action, int interval, bool registerToManager = false, string? registerName = null)
        {
            NamedTimer timer = CreateTimer(interval, registerToManager, registerName);
            int times = 0;
            timer.Elapsed += delegate
            {
                if (action.Invoke(times++)) timer.Dispose();
            };
            timer.Start();
            return timer;
        }
    }
}
