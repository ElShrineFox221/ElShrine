namespace ElShrine.Async;

public interface IPageInfo
{
    //min=1
    public int PCount { get; }
    //min=0, max=PCount-1
    public int PIndex { get; }
    //min=1
    public int PCapacity { get; }
}
