using ElShrine.VisualTool.ProcessLauncher.EFile;
using System.Runtime.Serialization;

namespace ElShrine.VisualTool.ProcessLauncher.Model
{
    [DataContract]
    public class ExeInfo(string path = Const.EmptyStr) : IExeInfo
    {
        [DataMember] public string Name { get; set; } = string.Empty;
        [DataMember] public string Description { get; set; } = string.Empty;
        //Excute
        [DataMember] public string TargetPath { get; set; } = path;
        [DataMember] public string Arguments { get; set; } = string.Empty;
        [DataMember] public string WorkingDirectory { get; set; } = string.Empty;
        [DataMember] public bool RunAsAdmin { get; set; } = false;
        [DataMember] public string CompatibilityMode { get; set; } = "Not set";
        //
        [DataMember] public string IconPath { get; set; } = string.Empty;
        [DataMember] public int IconIndex { get; set; } = 0;
        [DataMember] public string LnkFilePath { get; set; } = string.Empty;
        //
        [DataMember] public ExeLaunchMode LaunchMode { get; set; } = ExeLaunchMode.Once;
        [DataMember] public int MaxRetryCount { get; set; } = 5;
    }
}
