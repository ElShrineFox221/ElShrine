namespace ElShrine.EFile
{
    public class FileDetail
    {
        public FileDetail() { }
        public FileDetail(string path)
        {
            var temp = new FileInfo(path);
            FileInfo = temp;
        }
        public FileStream Open(FileMode mode)
        {
            if (!IsValid) throw new("Invalid file infos.");
            bool createDirectory = !(mode == FileMode.Truncate || mode == FileMode.Truncate);
            if (createDirectory)
            {
                string directory = FileInfo.DirectoryName ?? Environment.CurrentDirectory;
                if (directory.IsNotEmpty() && !System.IO.Directory.Exists(directory)) System.IO.Directory.CreateDirectory(directory);
            }
            return FileInfo.Open(mode);
        }


        private string? name = null;
        private string? extensionName = null;
        private string? directory = null;
        private string? fullPath = null;
        private FileInfo? fileInfo = null;

        public string Name
        {
            get => name ?? string.Empty;
            set
            {
                name = value;
                Update();
            }
        }
        public string ExtensionName
        {
            get => extensionName ?? string.Empty;
            set
            {
                extensionName = value;
                Update();
            }
        }
        public string Directory
        {
            get => directory ?? string.Empty;
            set
            {
                value = value.TrimEnd('\\');
                directory = value;
                Update();
            }
        }
        public string FullPath
        {
            get => fullPath ?? string.Empty;
            protected set => fullPath = value;
        }
        public FileInfo FileInfo
        {
            get => fileInfo ?? new(string.Empty);
            set
            {
                try
                {
                    FullPath = Path.GetFullPath(value.FullName);
                    fileInfo = value;
                    name = fileInfo.Name;
                    extensionName = fileInfo.Extension.Replace(".", string.Empty);
                    directory = fileInfo.DirectoryName;
                    IsValid = true;
                }
                catch
                {
                    IsValid = false;
                }
            }
        }
        protected virtual void Update()
        {
            if(name is not null && directory is not null)
            {
                var fp = string.IsNullOrEmpty(extensionName) ? $"{directory}\\{name}" : $"{directory}\\{name}.{extensionName}";
                try
                {
                    FullPath = Path.GetFullPath(fp);
                    fileInfo = new(FullPath);
                    IsValid = true;
                }
                catch
                {
                    IsValid = false;
                }
            }
        }
        public bool IsValid { get; protected set; }
    }
}
