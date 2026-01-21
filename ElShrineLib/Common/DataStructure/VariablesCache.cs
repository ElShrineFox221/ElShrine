using System.Collections;

namespace ElShrine.Common.DataStructure
{
    public sealed class VariablesCache<TValue, TPara> : IEnumerable<KeyValuePair<string, TValue>>, IValueProvider<string, TValue>
    {
        private readonly Dictionary<string, string> namesMap = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Func<TPara?, TValue>> varsByName = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string,  TValue?> cachedValuesByName = new(StringComparer.OrdinalIgnoreCase);
        private TPara? cachedPara = default; 

        public void Add(string name, Func<TPara?, TValue> func)
        {
            namesMap.Remove(name);
            varsByName[name] = func;
        }
        public bool Add(string prioName, string subPrioName)
        {
            if (prioName.EqualIgnoreCase(subPrioName)) return false;
            var hasPrioKey = varsByName.ContainsKey(prioName);
            var hasSubPrioKey = varsByName.ContainsKey(subPrioName);
            if (hasPrioKey && hasSubPrioKey)
            {
                varsByName.Remove(subPrioName);
                ClearCache(cachedPara);
            }
            if (hasPrioKey)
            {
                namesMap[subPrioName] = prioName;
                namesMap.Remove(prioName);
            }
            else if (hasSubPrioKey)
            {
                namesMap[prioName] = subPrioName;
                namesMap.Remove(subPrioName);
            }
            else return false;
            return true;
        }

        public TValue? this[string name]
        {
            get
            {
                var varName = GetVarName(name);
                if (cachedValuesByName.TryGetValue(varName, out var value)) return value;
                if (varsByName.TryGetValue(varName, out var func)) return cachedValuesByName[varName] = func(cachedPara);
                else return default;
            }
        }
        public string GetVarName(string name) => namesMap.TryGetValue(name, out var foundName) ? foundName : name;
        public void ClearCache(TPara? para)
        {
            cachedPara = para;
            cachedValuesByName.Clear();
        }
        public IEnumerator GetEnumerator() => GetEnumerator();

        IEnumerator<KeyValuePair<string, TValue>> IEnumerable<KeyValuePair<string, TValue>>.GetEnumerator()
        {
            foreach (var item in varsByName)
            {
                yield return new KeyValuePair<string, TValue>(item.Key, item.Value(cachedPara));
            }
        }
    }
}
