using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace ElShrine.EFile
{
    public static class LnkReader
    {
        #region Reader
        private class LocalExeInfo : IExeInfo
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            //Excute
            public string TargetPath { get; set; } = string.Empty;//taget | exe
            public string Arguments { get; set; } = string.Empty;
            public string WorkingDirectory { get; set; } = string.Empty;
            public bool RunAsAdmin { get; set; } = false;
            public string CompatibilityMode { get; set; } = "Not set";
            //
            public string IconPath { get; set; } = string.Empty;
            public int IconIndex { get; set; } = 0;
            public string LnkFilePath { get; set; } = string.Empty;
        }
        public static IExeInfo ReadLnkFile(string path, IExeInfo? refInfo = null)
        {
            //var path = refInfo.LnkFilePath;
            if (!File.Exists(path)) throw new FileNotFoundException("File not found", path);
            else
            {
                refInfo ??= new LocalExeInfo();
                if (Path.GetExtension(path).EqualIgnoreCase(".Lnk"))
                {

                    //Windows Script Host
                    readBasicInfo(path, refInfo);
                    //Shell32
                    readAdvancedInfo(path, refInfo);
                }
                else if (Path.GetExtension(path).EqualIgnoreCase(".Exe"))
                {
                    refInfo.Name = Path.GetFileNameWithoutExtension(path);
                    //desc
                    refInfo.TargetPath = path;
                }
                else throw new FileNotFoundException("File extension mismatched", path);
            }
            return refInfo;
            static void readBasicInfo(string path, IExeInfo info)
            {
                //Create
                Type shellType = Type.GetTypeFromProgID("WScript.Shell") ?? throw new("Found no shell class.");
                dynamic shell = Activator.CreateInstance(shellType) ?? throw new("Failed build no shell class.");
                dynamic shortcut = shell.CreateShortcut(path);
                //
                info.Name = Path.GetFileNameWithoutExtension(path);
                info.Description = shortcut.Description;
                info.TargetPath = shortcut.TargetPath;
                info.Arguments = shortcut.Arguments;
                info.WorkingDirectory = shortcut.WorkingDirectory;
                var Iconloc = shortcut.IconLocation;
                {
                    var iconParts = Iconloc.Split(',');
                    string iconPath = iconParts[0].Trim('"');
                    int iconIndex = iconParts.Length > 1 ? int.Parse(iconParts[1]) : 0;
                    info.IconPath = iconPath;
                    info.IconIndex = iconIndex;
                }
                //
                Marshal.FinalReleaseComObject(shortcut);
                Marshal.FinalReleaseComObject(shell);
            }
            static void readAdvancedInfo(string path, IExeInfo info)
            {
                //Create
                Type shellType = Type.GetTypeFromProgID("Shell.Application") ?? throw new("Found no shell class.");
                dynamic shell = Activator.CreateInstance(shellType) ?? throw new("Failed build no shell class.");
                //
                string folderPath = Path.GetDirectoryName(path) ?? throw new("Found no shell class.");
                string fileName = Path.GetFileName(path);
                dynamic folder = shell.NameSpace(folderPath);
                dynamic fileItem = folder.ParseName(fileName);
                //
                dynamic link = fileItem.GetLink();
                //
                try
                {
                    byte[] content = File.ReadAllBytes(path);
                    if (content.Length > 21) info.RunAsAdmin = (content[21] & 0x20) != 0;
                }
                catch { }
                info.CompatibilityMode = GetCompatibilityMode(link);
                //
                Marshal.FinalReleaseComObject(link);
                Marshal.FinalReleaseComObject(fileItem);
                Marshal.FinalReleaseComObject(folder);
                Marshal.FinalReleaseComObject(shell);
                //
                //
                static string GetCompatibilityMode(dynamic link)
                {
                    try
                    {
                        return link.Target.GetProperty("System.Link.CompatibilityMode");
                    }
                    catch
                    {
                        return "Not set";
                    }
                }
            }
        }

        public static bool Reload(this IExeInfo info)
        {
            var result = false;
            var path = info.TargetPath;
            if (File.Exists(path))
            {
                ReadLnkFile(path, info);
                result = true;
            }
            return result;
        }
        #endregion

        #region Icon
        private static readonly Icon DefaultIcon = SystemIcons.Application;
        public static Icon GetExeIcon(this IExeInfo info)
        {
            Icon icon = DefaultIcon;
            if (info.IconPath.IsNotEmpty()) icon = GetFileIcon(info.IconPath, info.IconIndex);
            else if (info.TargetPath.IsNotEmpty()) icon = GetFileIcon(info.TargetPath, 0);
            return icon;
        }
        public static Icon GetFileIcon(string filePath, int iconIndex = 0)
        {
            Icon result = DefaultIcon;
            if (Path.GetExtension(filePath).EqualIgnoreCase(".Lnk"))
            {
                var localLnkInfo = ReadLnkFile(filePath);
                result = getShortcutIcon(localLnkInfo);
            }
            else
            {
                var icon = extractIconFromFile(filePath, iconIndex);
                if (icon is not null) result = icon;
            }
            return result;
            static Icon? extractIconFromFile(string filePath, int iconIndex)
            {
                Icon? result = null;
                if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
                {
                    IntPtr[] largeIcons = new IntPtr[1];
                    int count = ExtractIconEx(filePath, iconIndex, largeIcons, [], 1);
                    if (count > 0 && largeIcons[0] != IntPtr.Zero) result = Icon.FromHandle(largeIcons[0]);
                }
                try
                {
                    result = Icon.ExtractAssociatedIcon(filePath);
                }
                catch { }
                return result;
            }
            static Icon getShortcutIcon(IExeInfo info)
            {
                Icon result = DefaultIcon;
                try
                {
                    bool finished = false;
                    //Directly
                    if (!string.IsNullOrEmpty(info.IconPath))
                    {
                        var icon = extractIconFromFile(info.IconPath, info.IconIndex);
                        if (icon is not null)
                        {
                            result = icon;
                            finished = true;
                        }
                    }
                    //Recursion
                    if (!finished && !string.IsNullOrEmpty(info.TargetPath))
                    {
                        string expandedPath = Environment.ExpandEnvironmentVariables(info.TargetPath);
                        //Special check
                        if (expandedPath.StartsWith("::", StringComparison.Ordinal))
                        {
                            try
                            {
                                //SHGetFileInfo
                                SHFILEINFO shinfo = new();
                                IntPtr hImg = SHGetFileInfo(expandedPath, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_LARGEICON);
                                if (shinfo.hIcon != IntPtr.Zero) result = Icon.FromHandle(shinfo.hIcon);
                            }
                            catch { }
                        }
                        return GetFileIcon(expandedPath);
                    }
                }
                catch { }
                return result;
            }
        }
        #endregion

        #region API Invoke

        #region Shell
        [DllImport("shell32.dll")]
        private static extern int ExtractIconEx(string lpszFile, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, int nIcons);

        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [StructLayout(LayoutKind.Sequential)]
        struct SHFILEINFO
        {
            public IntPtr hIcon; 
            public int iIcon; 
            public uint dwAttributes; 
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szDisplayName; 
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public string szTypeName;
        };

        private const uint SHGFI_ICON = 0x000000100;
        private const uint SHGFI_LARGEICON = 0x000000000;
        private const uint SHGFI_SMALLICON = 0x000000001;
        #endregion

        #endregion
    }
}
