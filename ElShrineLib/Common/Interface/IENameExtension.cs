namespace ElShrine.Common.Interface
{
    public static class IENameExtension
    {
        public static string GetDirectory(this IEFileNamed iefn)
            => iefn.ManualDirectory ?? $"{Environment.CurrentDirectory}";
        public static string GetFileName(this IEFileNamed iefn)
            => iefn.ManualFileName ?? (iefn.Name.IsEmpty() ? "DataHandler.NewFileName" : iefn.Name);
        public static string GetIdentifiedPath(this IEFileNamed iefn)
            => $"{iefn.GetDirectory()}\\{iefn.GetFileName()}";

        public static string GetDirectory<T>(this IEFileNamed<T> iefn)
            => iefn.ManualDirectory ?? $"{Environment.CurrentDirectory}\\{typeof(T).Name}";
        public static string GetFileName<T>(this IEFileNamed<T> iefn)
            => iefn.ManualFileName ?? (iefn.Name.IsEmpty() ? $"New{typeof(T).Name}File" : iefn.Name);
        public static string GetIdentifiedPath<T>(this IEFileNamed<T> iefn)
            => $"{iefn.GetDirectory()}\\{iefn.GetFileName()}";
    }
}
