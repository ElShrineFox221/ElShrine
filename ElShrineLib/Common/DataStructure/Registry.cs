namespace ElShrine.Common.DataStructure
{
    public class Registry<TKey, TValue> : IRegistry<TKey, TValue>, ICloneable<Registry<TKey, TValue>> where TKey : notnull
    {
        protected readonly Dictionary<TKey, TValue> Storage = [];
        public IReadOnlyDictionary<TKey, TValue> Items => Storage;
        
        public virtual bool Register(TKey key, TValue value)
        {
            var exists = Storage.ContainsKey(key);
            if (exists) return false;
            Storage.Add(key, value);
            return true;
        }

        public virtual object Clone()
        {
            var clone = new Registry<TKey, TValue>();
            foreach (var kv in Storage) clone.Storage.Add(kv.Key, kv.Value);
            return clone;
        }
    }
}
