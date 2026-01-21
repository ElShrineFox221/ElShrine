namespace ElShrine.Common.DataStructure
{
    public interface IRegistry<TKey, TValue>
    {
        IReadOnlyDictionary<TKey, TValue> Items { get; }
        bool Register(TKey key, TValue value);
        bool TryGetValue(TKey key, out TValue? value) => Items.TryGetValue(key, out value);
    }
}
