using System.Text;
using System.Text.Json;

namespace ElShrine.Modules.Log;

public static class LogStructureRepairer
{
    private class ReconstructNode(LogEntryData data)
    {
        public LogEntryData Data = data;
        public List<ReconstructNode> Children = [];
        public long? DurationMs { get; set; }
    }
    public static void Reconstruct(string rawLogPath, string? outputPath = null)
    {
        outputPath ??= rawLogPath.Replace(".raw.log", ".structured.log");
        var nodes = new Dictionary<long, ReconstructNode>();
        var nodesToStructure = new Dictionary<long, ReconstructNode>();
        var roots = new List<ReconstructNode>();

        using (var fs = new FileStream(rawLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var sr = new StreamReader(fs))
        {
            while (sr.ReadLine() is { } line)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    var data = JsonSerializer.Deserialize<LogEntryData>(line);
                    if (data == null) continue;

                    var node = new ReconstructNode(data);
                    nodes[data.Id] = node;

                    if (data.ParentId == 0) roots.Add(node);
                    else if (nodes.TryGetValue(data.ParentId, out var parent)) parent.Children.Add(node);
                    else nodesToStructure[data.Id] = node;
                }
                catch { /* 自动跳过损坏的 JSON 行 */ }
            }
        }
        if (roots.Count == 0)
        {
            var minDepth = nodesToStructure.Values.Min(n => n.Data.Depth);
            var selecteds = nodesToStructure.Values.Where(n => n.Data.Depth == minDepth).Select(n => n.Data.Id);
            foreach (var entryId in selecteds)
            {
                roots.Add(nodes[entryId]);
                nodesToStructure.Remove(entryId);
            }
        }

        foreach (var node in nodes.Values.Where(n => n.Data.Type == nameof(LogScope)))
        {
            var endNode = node.Children.FirstOrDefault(c => c.Data.IsEndOfScope);
            if (endNode != null)
            {
                node.DurationMs = endNode.Data.Timestamp - node.Data.Timestamp;
            }
        }

        using var sw = new StreamWriter(outputPath, false, Encoding.UTF8);
        foreach (var root in roots)
        {
            RenderNode(sw, root, 0);
        }
    }
    private static void RenderNode(StreamWriter sw, ReconstructNode node, int indentLevel)
    {
        var data = node.Data;

        string timePart = DateTimeOffset.FromUnixTimeMilliseconds(data.Timestamp).ToLocalTime().ToString("HH:mm:ss.fff");
        string typePart = data.Type.PadRight(12);
        string durPart = node.DurationMs.HasValue ? $"{node.DurationMs.Value,7}ms" : new string(' ', 9);

        string header = $"[{timePart}] [{typePart}] [{durPart}] | ";
        int headerWidth = header.Length;

        string indent = new(' ', indentLevel * 2);
        string symbol = data.Type == "LogScope" ? "▼ " : (data.IsEndOfScope ? "└─ " : "├─ ");
        int contentOffset = indent.Length + symbol.Length;

        string[] summaryLines = (data.Summary ?? "").Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < summaryLines.Length; i++)
        {
            if (i == 0)
                sw.WriteLine($"{header}{indent}{symbol}{summaryLines[i]}");
            else
            {
                sw.Write(new string(' ', headerWidth));
                sw.Write(new string(' ', contentOffset));
                sw.WriteLine(summaryLines[i]);
            }
        }
        if (data.Type == nameof(LogScope))
        {
            var orderedChildren = node.Children.OrderBy(c => c.Data.Id).ToList();
            foreach (var child in orderedChildren)
                RenderNode(sw, child, indentLevel + 1);

            if (orderedChildren.Count > 0 && !orderedChildren.Any(c => c.Data.IsEndOfScope))
            {
                sw.Write(new string(' ', headerWidth));
                sw.WriteLine($"{indent}  [!] TERMINATED_OR_CRASHED");
            }
        }
    }
}
