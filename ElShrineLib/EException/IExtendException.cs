namespace ElShrine.EException
{
    public interface IExtendException
    {
        public string LongMessage => $"{ShortMessage} {ExtraMessage}";
        public string ShortMessage { get; }
        public string ExtraMessage { get; }
    }
}
