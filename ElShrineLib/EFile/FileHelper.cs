namespace ElShrine.EFile
{
    public static class FileHelper
    {
        public static bool ExistFile(string path, string fileName, string extendName, bool extendIgnoreCase = false)
        {
            bool result = false;
            if (Directory.Exists(path))
            {
                FileInfo[] fileInfos = new DirectoryInfo(path).GetFiles();
                bool exist = fileInfos.Any(fi =>
                {
                    bool b0 = fi.Name.Replace(fi.Extension, Const.EmptyStr) == fileName;
                    bool b1 = fi.Extension.Equals($".{extendName}", extendIgnoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
                    return b0 && b1;
                });
                result = exist;
            }
            return result;
        }
    }
}
