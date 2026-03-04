namespace ElShrine.Modules.Log;
public interface ILogListener
{
    void InitializeEntries(IReadOnlyDictionary<LogSession, IReadOnlyDictionary<long, LogEntry>> entriesExist);
    void OnEntryAdded(LogSession session, LogScopeAccessor parentScopeAccessor, LogEntry entry);
}
