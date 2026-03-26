namespace ElShrine.Common
{
    public class CollectionChangedEventArgs<TValue>(IEnumerable<TValue>? removed = null, IEnumerable<TValue>? added = null, IEnumerable<TValue>? modified = null) : EventArgs
    {
        public IReadOnlyCollection<TValue>? Removed { get; init; } = removed is null ? [] : [.. removed];
        public IReadOnlyCollection<TValue>? Added { get; init; } = added is null ? [] : [.. added];
        public IReadOnlyCollection<TValue>? Modified { get; init; } = modified is null ? [] : [.. modified];
    }
    public delegate void CollectionChangedHandler<TValue>(object? sender, CollectionChangedEventArgs<TValue> e);
}
