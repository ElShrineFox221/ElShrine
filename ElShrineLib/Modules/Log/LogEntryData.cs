namespace ElShrine.Modules.Log;

public record class LogEntryData(
    long Id,
    long ParentId,
    int ThreadId,
    int Depth,
    long Timestamp,
    bool IsScope,
    bool IsEndOfScope,
    string Type,
    string Summary);