namespace ElShrine.Common.DataStructure
{
    public interface IValueProvider<TKey,TValue>
    {
        public TValue? this[TKey key] { get; }
        public bool TryGetValue(TKey key, out TValue? value)
        {
            value = this[key];
            return value != null;
        }
    }
}
