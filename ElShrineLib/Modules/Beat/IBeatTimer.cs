namespace ElShrine.Modules.Beat;

[Obsolete("To be deleted.")]
public interface IBeatTimer
{
    IDisposable RegisterListner(long intervalTicks);
}
[Obsolete("To be deleted.")]
public interface ITimerListener
{
    void Elasped(long ticks);
}