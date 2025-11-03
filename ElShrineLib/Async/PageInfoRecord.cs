namespace ElShrine.Async
{
    public record PageInfoRecord(int PCount, int PIndex, int PCapacity) : IPageInfo;
}
