using ElShrine.EConsole;

namespace ElShrine.VisualTool.Modules.Console
{
    public static class InfoHelper
    {
        public static ConsoleColor ToConsoleColor(this InformationPaintStyle paintStyle)
            => paintStyle switch
            {
                InformationPaintStyle.Sub => ConsoleColor.Gray,
                InformationPaintStyle.Complete => ConsoleColor.Green,
                InformationPaintStyle.SubComplete => ConsoleColor.DarkGreen,
                InformationPaintStyle.Warning => ConsoleColor.Yellow,
                InformationPaintStyle.SubWarning => ConsoleColor.Yellow,
                InformationPaintStyle.Error => ConsoleColor.Red,
                InformationPaintStyle.SubError => ConsoleColor.DarkRed,
                InformationPaintStyle.ParameterMethod => ConsoleColor.Cyan,
                InformationPaintStyle.SubParameterMethod => ConsoleColor.DarkCyan,
                _ => ConsoleColor.White
            };
    }
}
