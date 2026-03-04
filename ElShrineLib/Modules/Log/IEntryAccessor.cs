namespace ElShrine.Modules.Log;

public interface IEntryAccessor
{
    public long Id { get; }
    public long Timestamp { get; }
    public DateTime DateTime  => DateTimeOffset.FromUnixTimeMilliseconds(Timestamp).DateTime.ToLocalTime();
    public int ThreadId { get; }
    public int Depth { get; }
    public bool IsEndOfScope { get; }
    public string GetSummary();
    public string GetEntryType();
}