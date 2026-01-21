using ElShrine.Modules;

namespace ElShrine.Options
{
    [Option]
    public static class ConsoleOption
    {
        [OptionItem(Description = "Insert space line at command chunk end.")]
        public static bool UseSpaceLine { get; set; } = true;
        [OptionItem(Description = "Decide the command chunk depth of space line inserted.")]
        public static int SpaceLineLevel { get; set; } = 0;
        [OptionItem(Description = "Show time spend in command chunk.")]
        public static bool ShowSpendTime { get; set; } = true;
    }
}
