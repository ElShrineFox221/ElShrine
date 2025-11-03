namespace ElShrine.Async
{
    public class RefContainer<TContent>(TContent? content = null) where TContent : class
    {
        public TContent? Content { get; set; } = content;
    }
}
