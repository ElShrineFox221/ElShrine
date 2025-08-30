namespace ElShrine.EOption
{
    [Option(Name = "File")]
    public sealed class FileOption : ISingleton<FileOption>
    {
        private static FileOption? Instance = null;
        public static FileOption GetInstance() => Instance ??= new();

        [OptionItem(Description = "Directly delete files or move to recycle bin.")] public bool DirectlyDel = false;
        [OptionItem(Description = "Directory of log files.")] public string LogDir { get; set; } = "Logs";
        [OptionItem(Description = "Auto save options' changes.")] public bool AutoSave { get; set; } = true;
        [OptionItem(Description = "Auto view options when changes happen.")] public bool ViewChanges { get; set; } = false;
        public bool AutoSaveInstanceListChanges { get; set; } = false;
        public bool AutoViewInstanceListChanges { get; set; } = false;
    }
}
