namespace ElShrine.EFile
{
    public interface IExeInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        //Excute
        public string TargetPath { get; set; }//taget | exe
        public string Arguments { get; set; }
        public string WorkingDirectory { get; set; }
        public bool RunAsAdmin { get; set; }
        public string CompatibilityMode { get; set; }
        //
        public string IconPath { get; set; }
        public int IconIndex { get; set; }
    }
}
