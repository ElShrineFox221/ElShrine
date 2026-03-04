namespace ElShrine.Modules.Log;

public interface ILogSessionListener
{
    void InitializeEntries(IReadOnlyDictionary<long, LogEntry> entriesExist);
    void OnEntryAdded(LogScopeAccessor parentScopeAccessor, LogEntry entry);
}