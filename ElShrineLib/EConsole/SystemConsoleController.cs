using System.Runtime.InteropServices;
using System.Text;

namespace ElShrine.EConsole
{
    public class SystemConsoleController : IConsoleListener
    {

        #region Import system sdk
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeConsole();

        #endregion
        public static bool Open() => AllocConsole();
        public static void Close() => FreeConsole();
        public bool PrintInfoLine(InformationLine line, int listenerIndex)
        {
            if (ConsoleManager.InstantPrint)
            {
                Open();
                ConsoleWrite(line);
            }
            else
            {
                Task.Run(() =>
                {
                    Open();
                    ConsoleWrite(line);
                });
            }
            return true;
        }
        public const string IntentSpace = "   ";
        private static string GetIntent(int num)
        {
            StringBuilder builder = new();
            while (num-- > 0) builder.Append(IntentSpace);
            return builder.ToString();
        }
        private static void ConsoleWrite(InformationLine line)
        {
            if (line.LineText is not null) ColorfulWhiteLine(line.LineText, line.BasePaintStyle, line.Intent);
            else
            {
                ColorfulWhite(GetIntent(line.Intent), line.BasePaintStyle);
                var length = line.LineTextSources.Length;
                for(int i = 0; i< line.LineTextSources.Length; i++)
                {
                    var lineItem = line.LineTextSources[i];
                    var text = lineItem.PadTo > 0 ? lineItem.Text.PadRight(lineItem.PadTo) : lineItem.Text.PadLeft(Math.Abs(lineItem.PadTo));
                    if (lineItem.PaintStyle == InformationPaintStyle.None) lineItem.PaintStyle = line.BasePaintStyle;
                    if (i != length - 1) ColorfulWhite(text, lineItem.PaintStyle);
                    else ColorfulWhiteLine(text, lineItem.PaintStyle);
                }
            }
        }
        private static void ColorfulWhiteLine(string text, InformationPaintStyle paintStyle, int intent = 0)
        {
            var foreColorRecord = System.Console.ForegroundColor;
            System.Console.ForegroundColor = ColorTransfer(paintStyle);
            System.Console.WriteLine($"{GetIntent(intent)}{text}");
            System.Console.ForegroundColor = foreColorRecord;
        }
        private static void ColorfulWhite(string text, InformationPaintStyle paintStyle)
        {
            var foreColorRecord = System.Console.ForegroundColor;
            System.Console.ForegroundColor = ColorTransfer(paintStyle);
            System.Console.Write(text);
            System.Console.ForegroundColor = foreColorRecord;
        }

        private static ConsoleColor ColorTransfer(InformationPaintStyle paintStyle)
            => paintStyle switch
            {
                InformationPaintStyle.Sub => ConsoleColor.Gray,
                InformationPaintStyle.Complete => ConsoleColor.Green,
                InformationPaintStyle.SubComplete => ConsoleColor.DarkGreen,
                InformationPaintStyle.Warning => ConsoleColor.Yellow,
                InformationPaintStyle.SubWarning => ConsoleColor.DarkYellow,
                InformationPaintStyle.Error => ConsoleColor.Red,
                InformationPaintStyle.SubError => ConsoleColor.DarkRed,
                InformationPaintStyle.ParameterMethod => ConsoleColor.Cyan,
                InformationPaintStyle.SubParameterMethod => ConsoleColor.DarkCyan,
                _ => ConsoleColor.White
            };
    }
}
