using ElShrine.Common.Interface;

namespace ElShrine.ETimer
{
    public class NamedTimer(int interval, bool registered, string name) : System.Timers.Timer(interval), IEName
    {
        public string Name { get; init; } = name;
        public bool Registered { get; init; } = registered;
        public bool Ticking { get; protected set; } = false;
        public new void Start()
        {
            base.Start();
            Ticking = true;
        }
        public new void Stop()
        {
            Ticking = false;
            base.Stop();
        }
        public readonly DateTime TimeCreated = DateTime.UtcNow;
        public new void Dispose()
        {
            Stop();
            base.Dispose();
        }
    }
}
