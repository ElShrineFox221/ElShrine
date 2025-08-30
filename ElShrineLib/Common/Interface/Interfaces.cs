namespace ElShrine.Common.Interface
{
    

    public interface IEFileNamed : IEName
    {
        public string? ManualDirectory { get; set; }
        public string? ManualFileName { get; set; }
    }
    public interface IEFileNamed<T> : IEFileNamed { }

    public interface IEDirtable
    {
        public bool Dirtied { get; set; }
    }

    public interface IEListContainer<T>
    {
        List<T> Items { get; }
    }
}
