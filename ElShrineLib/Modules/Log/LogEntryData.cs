namespace ElShrine.Modules.Log;

public record class LogEntryData(long Id, long ParentId, int Depth, long Timestamp, bool IsEndOfScope, string Type, string Summary);