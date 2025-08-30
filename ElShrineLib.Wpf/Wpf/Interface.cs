namespace ElShrine.Wpf
{
    public interface IEVMEquatabe<ViewModel>
    {
        public bool DataEqual(ViewModel other);
    }
    public interface IEVMCollection
    {
        public VMCommand Add { get; }
        public VMCommand Remove { get; }
        public VMCommand Clear { get; }
        public VMCommand Reorder { get; }
    }
}
