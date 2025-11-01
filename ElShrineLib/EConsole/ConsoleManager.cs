using ElShrine.ECommand;
using ElShrine.EFile;
using ElShrine.EOption;
using ElShrine.ETimer;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Timers;

namespace ElShrine.EConsole
{
    [StartupClass]
    public static class ConsoleManager
    {
        private enum LineReportType { Normal, Error, Warning }
        private class InfoQueue : IEnumerable
        {
            public class MethodListInfoListener : IListInfoListener
            {
                public Stopwatch Watcher = Stopwatch.StartNew();
                public Queue<string> Warnings { get; set; } = [];
                public Queue<Exception> Errors { get; set; } = [];
            }
            private readonly List<InformationLine> Lines = [];
            public readonly MethodListInfoListener BaseListener = new();
            public readonly Stack<MethodListInfoListener> ChunkStack = [];
            public int Count => Lines.Count;
            public InformationLine? QueueHead => Count > 0 ? Lines[0] : null;
            public InformationLine? QueueTail => Count > 0 ? Lines[^1] : null;
            public void Enqueue(InformationLine line, LineReportType reportType = LineReportType.Normal)
            {
                Lines.Add(line);
                line.ChunkMiliseconds = ChunkStack.Count > 0 ? ChunkStack.Peek().Watcher.ElapsedMilliseconds : -1;
                switch (line.LineType)
                {
                    case InformationLineType.ChunkBegin:
                        line.AutoIntent = ChunkStack.Count;
                        line.ChunkMiliseconds = 0;
                        ChunkStack.Push(new());
                        break;
                    case InformationLineType.ChunkEnd:
                        ChunkStack.Pop().Watcher.Stop();
                        line.AutoIntent = ChunkStack.Count;
                        break;
                    default:
                    case InformationLineType.Normal:
                        line.AutoIntent = ChunkStack.Count;
                        break;
                }
                
                var listener = ChunkStack.Count > 0 ? ChunkStack.Peek() : BaseListener;
                switch (reportType)
                {
                    default:
                    case LineReportType.Normal:
                        break;
                    case LineReportType.Error:
                        listener.Errors.Enqueue(new());
                        break;
                    case LineReportType.Warning:
                        listener.Warnings.Enqueue(line.ToString());
                        break;
                }
            }
            public InformationLine? Dequeue()
            {
                var line = QueueHead;
                if(QueueHead is not null) Lines.Remove(QueueHead);
                return line;
            }
            public IEnumerator GetEnumerator() => Lines.GetEnumerator();
        }

        private readonly static InfoQueue InfoPrintQueue = [];
        private readonly static NamedTimer ConsoleInfosTimer = NamedTimerManager.CreateTimer(100, false, Const.EmptyStr);
        static ConsoleManager()
        {
            ConsoleInfosTimer.Elapsed += ControllerInvoker;
        }
        private static void ControllerInvoker(object? sender, ElapsedEventArgs? e)
        {
            if (InstantPrint)
            {
                if (sender is not null) return;
            }
            else
            {
                if (Paused) return;
                if (timerFinishedInvoke) timerFinishedInvoke = false;
                else return;
            }
            

            ConsoleInfosTimer.Interval = 100 + 300 / (1 + InfoPrintQueue.Count);
            bool queueable = false;
            if (InfoPrintQueue.Count > 0)
            {
                queueable = true;
                var line = InfoPrintQueue.QueueHead;
                if (line is not null)
                {
                    _optionApplier(line);
                    //
                    var queued = Controller?.PrintInfoLine(line, -1) ?? false;
                    if (queued)
                    {
                        Log(line);
                        for (int j = 0; j < Listeners.Count; j++)
                        {
                            Listeners[j].PrintInfoLine(line, j);
                        }
                        InfoPrintQueue.Dequeue();
                    }
                }
            }
            if (!queueable) Paused = true;
            timerFinishedInvoke = true;

            static void _optionApplier(InformationLine line)
            {
                var opt = ConsoleOption.GetInstance();
                if (line.LineType == InformationLineType.ChunkEnd && opt.UseSpaceLine && ConsoleOption.GetInstance().SpaceLineLevel >= line.AutoIntent)
                {
                    if (line.LineText is not null) line.LineText += '\n';
                    else line.LineTextSources = [.. line.LineTextSources, new("\n")];
                }
                if (line.LineType == InformationLineType.ChunkEnd && opt.ShowSpendTime && line.ChunkMiliseconds != -1)
                {
                    var notice = $" {line.ChunkMiliseconds} ms consumed.";
                    if (line.LineText is not null) line.LineText += notice;
                    else line.LineTextSources = [.. line.LineTextSources, new(notice, InformationPaintStyle.Sub)];
                }
            }
        }
        private static bool timerFinishedInvoke = true;
        public static bool Paused { get; set; } = true;

        private readonly static string InitializeTime = DateTime.Now.ToLocalTime().ToString(Const.FullDateTimeFormat).Replace(':', '\'');
        public static string? CurrentInitializeDirectory { get; private set; } = null;
        public static string? CurrentInitializePath { get; private set; } = null; 
        private static void Log(InformationLine line)
        {
            CurrentInitializeDirectory ??= FileOption.GetInstance().LogDir;
            var path = CurrentInitializePath??= $"{FileOption.GetInstance().LogDir}\\Log[{InitializeTime}].txt";
            var text = line.ToString();
            var fd = new FileDetails(path);
            if(fd.IsValid)
            {
                using FileStream fs = fd.Open(FileMode.OpenOrCreate, FileAccess.Write);
                {
                    fs.Position = fs.Length;
                    fs.Write(Encoding.UTF8.GetBytes($"{text}\n"));
                }
                fs.Close();
            }
        }

        #region Listeners and Controller
        
        private static IConsoleListener? controller = null;
        public static IConsoleListener? Controller => controller;
        public static void SetController(IConsoleListener listener, bool removeToListeners = false)
        {
            if (controller != listener)
            {
                Listeners.Remove(listener);
                if (removeToListeners && controller is not null && !Listeners.Contains(controller)) Listeners.Add(controller);
                controller = listener;
            }
        }
        public static List<IConsoleListener> Listeners { get; } = [];
        #endregion


        #region Directly Insert Line
        public static bool InstantPrint { get; set; } = false;
        #region Line or LineItem Builder
        public static InformationPaintStyle Subside(this InformationPaintStyle paintStyle, bool isSub)
            => isSub ? paintStyle switch
            {
                InformationPaintStyle.Warning => InformationPaintStyle.SubWarning,
                InformationPaintStyle.Error => InformationPaintStyle.SubError,
                InformationPaintStyle.ParameterMethod => InformationPaintStyle.SubParameterMethod,
                InformationPaintStyle.Complete => InformationPaintStyle.SubComplete,
                _ => InformationPaintStyle.Sub
            } : paintStyle;
        public static IListInfoListener GetListInfoListener()
        {
            InfoQueue.MethodListInfoListener stackItem;
            if (InfoPrintQueue.ChunkStack.Count > 0) stackItem = InfoPrintQueue.ChunkStack.Peek();
            else stackItem = InfoPrintQueue.BaseListener;
            return stackItem;
        }
        public static InformationItem GetWarningItem(bool? isSub = null) => new("[Warning]", InformationPaintStyle.Warning.Subside((isSub ?? DefaultSub)));
        public static InformationItem GetErrorItem(bool? isSub = null) => new("[Error]", InformationPaintStyle.Error.Subside((isSub ?? DefaultSub)));
        public static InformationItem GetCompleteItem(bool suc, bool? isSub = null)
            => suc ? new("[Completed]", InformationPaintStyle.Complete.Subside((isSub ?? DefaultSub))) : new("[Failed]", InformationPaintStyle.Faild.Subside((isSub ?? DefaultSub)));
        #endregion

        private static void ListInfo(InformationLine line, LineReportType reportType)
        {
            InfoPrintQueue.Enqueue(line, reportType);
            if (InstantPrint)
            {
                if(ConsoleInfosTimer.Ticking) ConsoleInfosTimer.Stop();
                ControllerInvoker(null, null);
            }
            else if (!ConsoleInfosTimer.Ticking) ConsoleInfosTimer.Start();
            if (Paused) Paused = false;
        }
        public static void ListInfo(InformationLine line)
            => ListInfo(line, LineReportType.Normal);
        public static void ListContentInfo(InformationItem[] items, bool? isSub = null)
            => ListInfo(new(items, InformationLineType.Normal, InformationPaintStyle.Normal.Subside(isSub ?? DefaultSub)), LineReportType.Normal);
        public static void ListContentInfo(string lineText, bool? isSub = null)
            => ListInfo(new(lineText, InformationLineType.Normal, InformationPaintStyle.Normal.Subside(isSub ?? DefaultSub)), LineReportType.Normal);
        public static void ListCommandNoticeInfo(string commandFormation, string actionDescription)
        {
            InformationItem item0 = new("Use command formation: ");
            InformationItem item1 = new(commandFormation, InformationPaintStyle.ParameterMethod);
            InformationItem item2 = new($" to {actionDescription}.");
            ListInfo(new([item0, item1, item2]), LineReportType.Normal);
        }
        public static void ListTableInfo(params (int extraPad, InformationItem[] items)[] columns)
        {
            for (int i = 0; i < columns.Length; i++)
            {
                var items = columns[i].items;
                var pads = items.Max(item => item.Text.Length) + columns[i].extraPad;
                foreach (var item in items) item.PadTo = pads;
            }
            var rows = columns.Max(col => col.items.Length);
            var cols = columns.Length;
            for (int i = 0; i < rows; i++)
            {
                var rowBuilder = new InformationItem[cols];
                for (int j = 0; j < cols; j++)
                {
                    var items = columns[j].items;
                    if (items.Length > i) rowBuilder[j] = columns[j].items[i];
                    else rowBuilder[j] = new(string.Empty);
                }
                var line = new InformationLine(rowBuilder) { IgnoreTime = i != 0 };
                ListInfo(line);
            }
        }

        public static bool DefaultSub { get; set; } = false;
        public static void ListBeginInfo(InformationItem[] items, bool? isSub = null, InformationPaintStyle basePaint = InformationPaintStyle.Normal)
            => ListInfo(new(items, InformationLineType.ChunkBegin, basePaint.Subside(isSub ?? DefaultSub)), LineReportType.Normal);
        public static void ListWarnInfo(InformationItem[] items, bool? isSub = null)
            => ListInfo(new(items, InformationLineType.Normal, InformationPaintStyle.Normal.Subside(isSub ?? DefaultSub)), LineReportType.Warning);
        public static void ListErrorInfo(InformationItem[] items, bool? isSub = null)
            => ListInfo(new(items, InformationLineType.Normal, InformationPaintStyle.Normal.Subside(isSub ?? DefaultSub)), LineReportType.Error);
        public static void ListErrorInfo(Exception exception, bool? isSub = null)
        {
            while(exception.InnerException is not null) exception = exception.InnerException;
            InformationLine line = new(exception.Message, InformationLineType.Normal, (isSub ?? DefaultSub) ? InformationPaintStyle.SubError : InformationPaintStyle.Error);
            ListInfo(line, LineReportType.Error);
        }
        public static void ListEndInfo(InformationItem[] items, bool? isSub = null, InformationPaintStyle basePaint = InformationPaintStyle.Normal)
            => ListInfo(new(items, InformationLineType.ChunkEnd, basePaint.Subside(isSub ?? DefaultSub)), LineReportType.Normal);
        #endregion
    }
}
