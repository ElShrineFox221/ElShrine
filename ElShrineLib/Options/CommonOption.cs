using ElShrine.Modules;

namespace ElShrine.Options
{
    [Option]
    public static class CommonOption
    {
        [OptionItem(Description = "Directly delete files or move to recycle bin.")]
        public static bool DirectlyDel { get; set; } = false;
        [OptionItem(Description = "Directory of log files.")]
        public static string LogDir { get; set; } = "Logs";
    }
}
