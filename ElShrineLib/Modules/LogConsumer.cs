using ElShrine.Modules.Log;

namespace ElShrine.Modules;

#region Consumer Implements
public class SystemConsoleLogger : ILogSessionListener
{
    private const bool DrakMode = true;
    private static readonly object _consoleLock = new();

    public void OnEntryAdded(LogScopeAccessor parentScope, LogEntry entry)
        => PrintLine(entry);
    private static void PrintLine(LogEntry entry)
    {
        lock (_consoleLock)
        {
            PrintPrefix(entry);
            if (entry.Depth > 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(new string(' ', 3 * entry.Depth));
            }
            if (entry is InfoEntry info)
            {
                var infos = info.Content;
                foreach (var item in infos.LogItems)
                {
                    PrintLogItem(item);
                }
            }
            Console.ResetColor();
            Console.WriteLine();
        }
    }
    private static void PrintPrefix(LogEntry entry)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"[{DateTimeOffset.FromUnixTimeMilliseconds(entry.Timestamp).LocalDateTime:HH:mm:ss.fff}] ");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write($"[T:{entry.ThreadId:D3}] ");
        var type = entry.EntryType;
        switch (type)
        {
            case "Error":
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" ERR ");
                break;
            case "Warning":
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(" WRN ");
                break;
            default: // Normal
                if (entry.IsEndOfScope)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write(" END ");
                    break;
                }
                else if (entry.IsScopeHeader)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(" BEG ");
                    break;
                }
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(" INF ");
                break;
        }
        Console.ResetColor();
        Console.Write(" ");
    }

    private static void PrintLogItem(LogItem item)
    {
        if (string.IsNullOrEmpty(item.Text)) return;

        var (fg, bg) = GetColors(item.Style, DrakMode);
        Console.ForegroundColor = fg;
        Console.BackgroundColor = bg;
        Console.Write(item.ToString());
        Console.ResetColor();
    }
    private static (ConsoleColor Foreground, ConsoleColor Background) GetColors(LogItemStyle style, bool isDrak)
    {
        // 默认为黑色背景
        ConsoleColor bg = isDrak ? ConsoleColor.Black : ConsoleColor.White;
        ConsoleColor fg = isDrak ? style switch
        {
            LogItemStyle.Info => ConsoleColor.White,
            LogItemStyle.SubInfo => ConsoleColor.Gray,
            LogItemStyle.Success => ConsoleColor.Green,
            LogItemStyle.Warning => ConsoleColor.Yellow,
            LogItemStyle.Error => ConsoleColor.Red,
            LogItemStyle.NoticeBlue => ConsoleColor.Blue,
            LogItemStyle.NoticePurple => ConsoleColor.Magenta,
            LogItemStyle.NoticePaleGreen => ConsoleColor.DarkGreen,
            LogItemStyle.NoticeDarkYellow => ConsoleColor.DarkYellow,
            LogItemStyle.NoticeCyan => ConsoleColor.Cyan,
            _ => ConsoleColor.White
        } : 
        style switch
        {
            LogItemStyle.Info => ConsoleColor.Black,
            LogItemStyle.SubInfo => ConsoleColor.DarkGray,
            LogItemStyle.Success => ConsoleColor.Green,
            LogItemStyle.Warning => ConsoleColor.Yellow,
            LogItemStyle.Error => ConsoleColor.Red,
            LogItemStyle.NoticeBlue => ConsoleColor.Blue,
            LogItemStyle.NoticePurple => ConsoleColor.Magenta,
            LogItemStyle.NoticePaleGreen => ConsoleColor.DarkGreen,
            LogItemStyle.NoticeDarkYellow => ConsoleColor.DarkYellow,
            LogItemStyle.NoticeCyan => ConsoleColor.Cyan,
            _ => ConsoleColor.Black
        };
        return (fg, bg);
    }
}
#endregion
