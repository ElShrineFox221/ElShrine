using ElShrine.EOption;

namespace ElShrine.Old.Console
{
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    //[EOption(Name = nameof(ConsoleOption))]
    public sealed class ConsoleOption : ISingleton<ConsoleOption>
    {
        public const string Space = "   ";
        public static ConsoleOption? Instance { get; set; }
        public static ConsoleOption GetInstance() => Instance ??= new();

        public bool PrintInfosByLine { get; set; } = true;

        public ConsoleColor DefaultColor { get; set; } = ConsoleColor.White;
        public ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;
        public ConsoleColor CompleteColor { get; set; } = ConsoleColor.Green;
        public ConsoleColor CommandColor { get; set; } = ConsoleColor.Yellow;
        public ConsoleColor ParameterColor { get; set; } = ConsoleColor.Cyan;
        public ConsoleColor MethodColor { get; set; } = ConsoleColor.DarkCyan;
        public ConsoleColor NoticeColor { get; set; } = ConsoleColor.Magenta;
    }
}
