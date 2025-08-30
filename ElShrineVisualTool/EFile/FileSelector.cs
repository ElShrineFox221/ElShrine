using ElShrine;
using Microsoft.Win32;
using System.IO;

namespace ElShrine.EFile
{
    public static class FileSelector
    {
        public const string AllFileFilter = "所有文件|*.*";
        public static string[] SelectFilesOrFolder(string filter = AllFileFilter)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "FolderSelection",
                Filter = filter
            };
            string[] result = [];
            if (dialog.ShowDialog() == true) result = [.. dialog.FileNames.Where(path => Directory.Exists(path) || File.Exists(path))];
            return result;
        }
        public static string SelectFolder(string initialDirectory = Const.EmptyStr)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select a folder",
                Multiselect = false,
            };
            if (initialDirectory.IsNotEmpty()) dialog.InitialDirectory = initialDirectory;
            var dialogResult = dialog.ShowDialog();
            return dialogResult == true ? dialog.FolderName : string.Empty;
        }
    }
}
