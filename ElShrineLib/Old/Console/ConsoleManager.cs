using ElShrine.Old.Command;
using ElShrine.ETimer;
using System.Runtime.InteropServices;

namespace ElShrine.Old.Console
{
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    [CommandCarrier(LoadMode = CommandAutoLoadMode.None, Name = "Console")]
    public static partial class ConsoleManager
    {

        #region Import system sdk
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool AllocConsole();

        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool FreeConsole();

        #endregion
        public static bool Open() => AllocConsole();
        public static void Close() => FreeConsole();

        static ConsoleManager()
        {
            ConsoleRead += (str) =>
            {
                StringHasRead += str;
                if (ReadLineCompleted)
                {
                    ReadLineCompleted = false;
                    ConsoleReadFullLine?.Invoke(StringHasRead);
                    StringHasRead = string.Empty;
                }
            };
            ConsoleWrite += (str) =>
            {
                StringHasWrite += str;
                if (WriteLineCompleted)
                {
                    WriteLineCompleted = false;
                    ConsoleWriteFullLine?.Invoke(StringHasWrite);
                    StringHasWrite = string.Empty;
                }
            };

            ConsoleInfosTimer.Elapsed += delegate
            {
                bool byLine = ConsoleOption.GetInstance().PrintInfosByLine;
                ConsoleInfosTimer.Interval = byLine ? 30 : 10 + 90 / (1 + InfosQueue.Count / 4);
                if (InfosQueue.Count > 0)
                {
                    Information info = DequeueInformation();
                    while (byLine && InfosQueue.Count > 0 && !info.FullLine) info = DequeueInformation();
                }
            };
            ConsoleInfosTimer.Start();

            EnqueueInfo += (info) =>
            {
                CurrentInfosLine.TryAdd(info, out bool success);
                if (!success || CurrentInfosLine.IsFullLine)
                {
                    EnqueueInformationLine(CurrentInfosLine);
                    CurrentInfosLine = new();
                }
            };
            EnqueueInfoLine += (infoLine) => Logs.Add(infoLine.GetFullLineWithTime(Const.FullDateTimeFormat) ?? Const.EmptyStr);
        }

        #region Console common write & read
        private static string StringHasRead = string.Empty;
        private static bool ReadLineCompleted = false;
        private static string StringHasWrite = string.Empty;
        private static bool WriteLineCompleted = false;
        public static ConsoleKeyInfo ReadKey()
        {
            var result = System.Console.ReadKey();
            ConsoleRead?.Invoke(result.KeyChar.ToString());
            return result;
        }
        public static string ReadLine()
        {
            string result = System.Console.ReadLine() ?? string.Empty;
            ReadLineCompleted = true;
            ConsoleRead?.Invoke(result);
            return result;
        }
        public static void Write(string text)
        {
            System.Console.Write(text);
            ConsoleWrite?.Invoke(text);
        }
        public static void WriteLine(string text)
        {
            System.Console.WriteLine(text);
            WriteLineCompleted = true;
            ConsoleWrite?.Invoke(text);
        }

        public delegate void ConsoleHandler(string text);
        public static event ConsoleHandler? ConsoleWrite;
        public static event ConsoleHandler? ConsoleRead;
        public static event ConsoleHandler? ConsoleReadFullLine;
        public static event ConsoleHandler? ConsoleWriteFullLine;
        #endregion

        #region Console advanced print
        private readonly static Queue<Information> InfosQueue = [];
        private readonly static Queue<InformationLine> InfosLineQueue = [];

        private static InformationLine CurrentInfosLine = new();
        public static void ListInfo(Information information)
            => EnqueueInformation(information);
        public static void ListInfo(Information information, params Information[] informations)
        {
            ListInfo(information);
            foreach(var info in informations) ListInfo(info);
        }

        public static void ListInfoCommandNotice(string commandFormation, string actionDescription)
        {
            ListInfo(new("Use command formation: ", InfosType.Normal, false));
            ListInfo(new($"<{commandFormation}>", InfosType.Method, false));
            ListInfo(new($" to {actionDescription}.", InfosType.Normal));
        }
        public static string? ReadLine(ConsoleColor? color = null)
            => Information.Read(color);

        private static InformationLine DequeueInformationLine()
        {
            InformationLine infoLine = InfosLineQueue.Dequeue();
            DequeueInfoLine?.Invoke(infoLine);
            return infoLine;
        }
        private static void EnqueueInformationLine(InformationLine informationLine)
        {
            InfosLineQueue.Enqueue(informationLine);
            EnqueueInfoLine?.Invoke(informationLine);
        }
        public delegate void ConsoleInfosLineQueueHandler(InformationLine infoLine);
        public static event ConsoleInfosLineQueueHandler? DequeueInfoLine;
        public static event ConsoleInfosLineQueueHandler? EnqueueInfoLine;
        private static Information DequeueInformation()
        {
            Information info = InfosQueue.Dequeue();
            info.Print();
            DequeueInfo?.Invoke(info);
            return info;
        }
        private static void EnqueueInformation(Information information)
        {
            InfosQueue.Enqueue(information);
            EnqueueInfo?.Invoke(information);
        }
        public delegate void ConsoleInfosQueueHandler(Information info);
        public static event ConsoleInfosQueueHandler? DequeueInfo;
        public static event ConsoleInfosQueueHandler? EnqueueInfo;
        #endregion

        #region CommandRead
        private readonly static NamedTimer ConsoleInfosTimer = NamedTimerManager.CreateTimer(100, false, Const.EmptyStr);
        public static void ReadCommandEntrance()
        {
            Process();
            static void Process()
            {
                PrintInputNotice();
                ExcuteCommand(out bool requireNext, out Command.Command command);

                if (requireNext) Process();
                else System.Console.ReadKey();
            }
        }
        public static void PrintInputNotice()
        {
            ListInfo(new(string.Empty));
            ListInfo(new("INPUT COMMAND >>> ", FullLine: false));
        }
        public static void ExcuteCommand(out bool requireNext, out Command.Command command)
        {
            var commandstr = Information.Read() ?? Const.EmptyStr;
            command = new(commandstr);
            command.Parser()?.Execute();
            requireNext = !command.CommandStr.Equals(nameof(GlobalCommand.Exit), StringComparison.CurrentCultureIgnoreCase);
        }
        #endregion

        private readonly static List<string> Logs = [];
        private readonly static string LogDirectory = $"{Environment.CurrentDirectory}\\Logs";
        [Command]
        public static void SaveLog()
        {
            //DataHandler.Print(Logs, $"Log[{DateTime.Now.ToString(Const.DateTimeFormat.Replace(":","\\\'"))}]", LogDirectory);
        }
        [Command]
        public static void OpenLog() => GlobalCommand.Open(LogDirectory);
    }
}
