using P = System.IO.Path;
using D = System.IO.Directory;

namespace ElShrine.Common
{
    public sealed class FileDetails() : IDisposable
    {
        private string path = string.Empty;
        private bool fileOpened = false;
        private FileStream? stream = null;
        private FileInfo? fileInfo = null;
        private FileInfo? FileInfo
        {
            get
            {
                if (fileInfo is null && IsValid)
                {
                    try
                    {
                        fileInfo = new(path);
                    }
                    catch (Exception)
                    {
                        fileInfo = null;
                    }
                }
                return fileInfo;
            }
        }
        public string Path
        {
            get => path;
            set
            {
                if (fileOpened) throw new InvalidOperationException("Cannot modify path while the file stream is open.");
                if (path == value) return;
                string oldPath = path;
                path = value;
                fileInfo = null;
                PathChanged?.Invoke(this, new(oldValue: oldPath, newValue: value));
            }
        }
        


        public event ValueChangedHandler<string>? PathChanged;

        public FileDetails(string path) : this() => Path = path;
        public FileDetails(FileInfo fileInfo) : this() => Path = (this.fileInfo = fileInfo).FullName;

        public string Extension
        {
            get => P.GetExtension(path) ?? string.Empty;
            set
            {
                if (fileOpened) throw new InvalidOperationException("Cannot modify path components while the file stream is open.");
                if (!IsValid) return;
                Path = P.Combine(Directory, FileNameWithoutExtension + value);
            }
        }
        public string FileName
        {
            get => P.GetFileName(path) ?? string.Empty;
            set
            {
                if (fileOpened) throw new InvalidOperationException("Cannot modify path components while the file stream is open.");
                if (!IsValid) return;
                Path = P.Combine(Directory, value);
            }
        }
        public string FileNameWithoutExtension
        {
            get => P.GetFileNameWithoutExtension(path) ?? string.Empty;
            set
            {
                if (fileOpened) throw new InvalidOperationException("Cannot modify path components while the file stream is open.");
                if (!IsValid) return;
                Path = P.Combine(Directory, value + Extension);
            }
        }
        public string Directory
        {
            get => P.GetDirectoryName(path) ?? string.Empty;
            set
            {
                if (fileOpened) throw new InvalidOperationException("Cannot modify path components while the file stream is open.");
                if (!IsValid) return;
                Path = P.Combine(value, FileName);
            }
        }

        public bool IsValid => !string.IsNullOrEmpty(path) && path.IndexOfAny(P.GetInvalidPathChars()) == -1;
        public bool IsExisted => IsValid && File.Exists(path);
        public string FullPath => IsValid ? P.GetFullPath(path) : string.Empty;
        public DateTime UpdatedDateTime => FileInfo?.LastWriteTime ?? DateTime.MinValue;
        public DateTime CreatedDateTime => FileInfo?.CreationTime ?? DateTime.MinValue;

        public FileStream Open(FileMode mode, FileAccess access)
        {
            if (!IsValid) throw new InvalidOperationException("Cannot open file stream. The path is invalid.");
            if (fileOpened) throw new InvalidOperationException("File stream is already open. Call EnsureClose() first.");
            if (!D.Exists(Directory)) D.CreateDirectory(Directory);
            stream = new FileStream(path, mode, access);
            fileOpened = true;
            return stream;
        }
        public bool EnsureClose()
        {
            bool wasOpened = fileOpened;
            stream?.Dispose();
            stream = null;
            fileOpened = false;
            return wasOpened;
        }

        public void Dispose()
        {
            EnsureClose();
            GC.SuppressFinalize(this);
        }
    }
}
