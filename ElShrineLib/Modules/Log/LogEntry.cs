namespace ElShrine.Modules.Log;

public abstract class LogEntry : IEntryAccessor
{
    private static long _nextEntryId = 0;
    public long Id { get; } = Interlocked.Increment(ref _nextEntryId);
    public int ThreadId { get; } = Environment.CurrentManagedThreadId;
    public long Timestamp { get; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public int Depth { get; internal set; } = 0;
    public bool IsEndOfScope { get; internal set; } = false;
    public abstract string GetSummary();
    public sealed override string ToString()
        => $"{GetEntryType()}: {GetSummary()}";
    public virtual string GetEntryType() 
        => GetEntryType(GetType());
    public static string GetEntryType<TEntry>() where TEntry : LogEntry
        => GetEntryType(typeof(TEntry));
    private static string GetEntryType(Type type)
    {
        var name = type.Name;
        if (name.EndsWith("Entry")) name = name[..^5];
        return name;
    }
}
