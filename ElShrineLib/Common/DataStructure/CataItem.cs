namespace ElShrine.Common.DataStructure
{
    public enum CataType
    {
        Actual,
        Virtual,
        Mixed,
    }
    public interface ICataItem
    {
        public string ActualCataName { get; }
        public string VirtualCataName { get; }
        public string ActualItemName { get; }
        public string VirtualItemName { get; }
    }
    public sealed record Cata(string CataName, int ItemsCount = -1)
    {
        public static implicit operator Cata(string cata) => new(cata, 0);
        public static implicit operator string(Cata cata) => cata.CataName;
    }
    public sealed class CataItemIndexer<T> where T : ICataItem
    {
        private readonly Dictionary<string, List<T>> mixedIndex = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<T>> virtualIndex = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<T>> actualIndex = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<T> source;

        public CataItemIndexer(IEnumerable<T> items)
        {
            source = [.. items];
            BuildIndex();
        }
        private void BuildIndex()
        {
            //build mix index, match both actual and virtual cataName itemName;
            mixedIndex.Clear();
            foreach (var item in source)
            {
                Register(item.ActualCataName, item);
                if (!string.Equals(item.ActualCataName, item.VirtualCataName, StringComparison.OrdinalIgnoreCase)) Register(item.VirtualCataName, item);
            }
            //build virtual index
            virtualIndex.Clear();
            var groupedVirtual = source.GroupBy(static item => item.VirtualCataName);
            foreach (var group in groupedVirtual) virtualIndex[group.Key] = [.. group];
            //build actual index
            actualIndex.Clear();
            var groupedActual = source.GroupBy(item => item.ActualCataName);
            foreach (var group in groupedActual) actualIndex[group.Key] = [.. group];
        }
        private void Register(string key, T item)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            if (!mixedIndex.TryGetValue(key, out var list)) mixedIndex[key] = list = [];
            list.Add(item);
        }
        private Dictionary<string, List<T>> GetSource(CataType cataType) =>
            cataType switch
            {
                CataType.Actual => actualIndex,
                CataType.Virtual => virtualIndex,
                CataType.Mixed => mixedIndex,
                _ => [],
            };

        public IReadOnlyList<T> Query(string cataName, string itemName, Predicate<T>? filter = null)
        {
            if (!mixedIndex.TryGetValue(cataName, out var candidates)) return [];
            return [.. candidates
                .Select((item, index) =>
                {
                    if(filter?.Invoke(item) is false) return  (Item: item, Score: -1, Index: index);
                    var actualNameMatched = string.Equals(item.ActualItemName, itemName, StringComparison.OrdinalIgnoreCase);
                    var actualCataMatched = string.Equals(item.ActualCataName, cataName, StringComparison.OrdinalIgnoreCase); 
                    var score = actualCataMatched ? 0 : 2;
                    if(!actualNameMatched)
                    {
                        var virtualNameMatched = string.Equals(item.VirtualItemName, itemName, StringComparison.OrdinalIgnoreCase);
                        if (virtualNameMatched) score += 1;
                        else return  (Item: item, Score: -1, Index: index);
                    }
                    return (Item: item, Score: score, Index: index);
                })
                .Where(r => r.Score >= 0)
                .OrderByDescending(r => r.Score)
                .ThenBy(r => r.Index)
                .Select(r => r.Item)];
        }
        public IReadOnlyList<T> QueryCata(string cataName, CataType cataType, Predicate<T>? filter = null)
        {
            var source = GetSource(cataType);
            var result = source.TryGetValue(cataName, out var list) ? filter is null ? list : [.. list.Where(li => filter(li))] : [];
            return result;
        }
        public IEnumerable<Cata> GetAllCatas(CataType cataType = CataType.Virtual, Predicate<IReadOnlyList<T>>? filter = null)
        {
            var source = GetSource(cataType);
            var result = source.Select(static kvp => new Cata(kvp.Key, kvp.Value.Count));
            if (filter is not null) result = [.. result.Where(cata => filter(source[cata.CataName]))];
            return result;
        }
        public IReadOnlyDictionary<Cata, IReadOnlyList<T>> GetAll(CataType groupedBy = CataType.Virtual, Predicate<IReadOnlyList<T>>? filter = null)
        {
            var source = GetSource(groupedBy);
            
            if(filter is null)
            {
                var result = source.ToDictionary(item => new Cata(item.Key, source[item.Key].Count), item => (IReadOnlyList<T>)source[item.Key]);
                return result;
            }
            else
            {
                var result = source.Where(i => filter(i.Value)).ToDictionary(item => new Cata(item.Key, source[item.Key].Count), item => (IReadOnlyList<T>)source[item.Key]);
                return result;
            }
        }
    }
}
