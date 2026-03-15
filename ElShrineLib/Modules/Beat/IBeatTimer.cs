namespace ElShrine.Modules.Beat;

public interface IBeatTimer
{
    IDisposable RegisterListner(long intervalTicks);
}

public interface ITimerListener
{
    void Elasped(long ticks);
}