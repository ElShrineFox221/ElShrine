namespace ElShrine.ECommand
{
    public interface IListInfoListener
    {
        public Queue<string> Warnings { get; }
        public Queue<Exception> Errors { get; }
    }
}
