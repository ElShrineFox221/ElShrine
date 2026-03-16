using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

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

public class LogWriter : ILogWriter
{
    private bool _disposed = false;
    private readonly ConcurrentDictionary<int, StreamWriter> _writers = [];
    public string LogBaseDirectory { get; }
    public string LogCurrentFolder { get; }
    private readonly string _curDir;
    public LogWriter()
    {
        // Path setup: App/Logs/Log[timestamp]/
        LogBaseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        LogCurrentFolder = $"Log[{DateTime.Now.ToLocalTime().ToString(Const.FullDateTimeFormat).Replace(':', '\'')}]";
        _curDir = (this as ILogWriter).LogCurrentDirectory;
        Directory.CreateDirectory(_curDir);
    }

    public void OnEntryAdded(ILogger session, LogScopeAccessor parentScopeAccessor, LogEntry sourceEntry)
    {
        if (_disposed)
            return;
        var data = new LogEntryData(
             sourceEntry.Id,
             parentScopeAccessor.Id,
             sourceEntry.ThreadId,
             sourceEntry.Depth,
             sourceEntry.Timestamp,
             sourceEntry is LogScope,
             sourceEntry.IsEndOfScope,
             sourceEntry.EntryType,
             sourceEntry.GetSummary());
        var json = JsonSerializer.Serialize(data);
        StreamWriter? writer;
        lock (_writers)
        {
            if (!_writers.TryGetValue(session.Id, out writer))
            {
                var path = Path.Combine(_curDir, $"{session.Name}.raw.log");
                var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.WriteThrough);
                _writers[session.Id] = writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            }
        }
        writer.WriteLine(json);
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        foreach (var writer in _writers.Values)
            writer.Dispose();
        _writers.Clear();
        GC.SuppressFinalize(this);
    }
}
