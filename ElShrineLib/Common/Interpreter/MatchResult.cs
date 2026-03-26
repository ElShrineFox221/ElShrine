using ElShrine.Common.DataStructure;

namespace ElShrine.Common.Interpreter
{
    public class MatchResult : IValueProvider<string, List<MatchResult>>
    {
        public ASTNode? ParsedNode { get; set; }
        public Dictionary<string, List<MatchResult>> MatchResults { get; set; } = [];
        public ASTNode? AsNode() => ParsedNode;
        public static implicit operator ASTNode?(MatchResult? result) => result?.ParsedNode;
        public static implicit operator MatchResult?(List<MatchResult> results) => results.Count > 0 ? results[0] : null;
        public List<MatchResult> this[string key] => MatchResults.TryGetValue(key, out var result) ? result : [];

        public void Merge(MatchResult other, string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (!MatchResults.TryGetValue(name, out var list)) MatchResults[name] = list = [];
                list.Add(other);
            }
            else
            {
                foreach (var item in other.MatchResults)
                {
                    if (MatchResults.TryGetValue(item.Key, out var elist)) elist.AddRange(item.Value);
                    else MatchResults[item.Key] = item.Value;
                }
                if(other.ParsedNode is not null && ParsedNode is null) ParsedNode = other.ParsedNode;
            }
        }
    }
}
