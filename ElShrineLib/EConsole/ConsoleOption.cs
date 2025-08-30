using ElShrine.EOption;

namespace ElShrine.EConsole
{
    [Option(Name = "Console")]
    public sealed class ConsoleOption : ISingleton<ConsoleOption>
    {
        private static ConsoleOption? Instance = null;
        public static ConsoleOption GetInstance() => Instance ??= new();

        [OptionItem(Description = "Insert space line at command chunk end.")]
        public bool UseSpaceLine { get; set; } = true;
        [OptionItem(Description = "Decide the command chunk depth of space line inserted.")]
        public int SpaceLineLevel { get; set; } = 0;
        [OptionItem(Description = "Show time spend in command chunk.")]
        public bool ShowSpendTime { get; set; } = true;
    }
}
