using ElShrine.Modules;

namespace ElShrine.Options
{
    [Option]
    public static class FileOption
    {
        [OptionItem(Description = "Directly delete files or move to recycle bin.")] 
        public static bool DirectlyDel { get; set; } = false;
        [OptionItem(Description = "Directory of log files.")] 
        public static string LogDir { get; set; } = "Logs";
        [OptionItem(Description = "Auto save options' changes.")] 
        public static bool AutoSave { get; set; } = true;
        [OptionItem(Description = "Auto view options when changes happen.")] 
        public static bool ViewChanges { get; set; } = false;
    }
}
