using System.Text;
using System.Text.Json;

namespace ElShrine.Modules.Log;

public static class LogStructureRepairer
{
    private class ReconstructNode(LogEntryData data)
    {
        public LogEntryData Data = data;
        public List<ReconstructNode> Children = [];
        public long DurationMs { get; set; } = -1;
    }
    private static readonly Comparison<ReconstructNode> _nodeComparer = static (a, b) => a.Data.Id.CompareTo(b.Data.Id);
    public static void Reconstruct(string rawLogPath, string? outputPath = null, string? customTermination = null)
    {
        outputPath ??= rawLogPath.Replace(".raw.log", ".structured.log");
        var allNodes = new Dictionary<long, ReconstructNode>();
        var roots = new List<ReconstructNode>();
        var config = new LayoutConfig { TerminationText = customTermination ?? LayoutConfig.Default.TerminationText };

        using (var fs = new FileStream(rawLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var sr = new StreamReader(fs))
        {
            while (sr.ReadLine() is { } line)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    var data = JsonSerializer.Deserialize<LogEntryData>(line);
                    if (data is null) continue;
                    var node = new ReconstructNode(data);
                    allNodes[data.Id] = node;
                }
                catch { /* 自动跳过损坏的 JSON 行 */ }
            }
        }

        foreach (var node in allNodes.Values)
        {
            if (node.Data.ParentId != 0 && allNodes.TryGetValue(node.Data.ParentId, out var parent))
                parent.Children.Add(node);
            else
                roots.Add(node);
        }
        roots.Sort(_nodeComparer);
        foreach (var node in allNodes.Values)
        {
            config.MaxTypeWidth = Math.Max(config.MaxTypeWidth, node.Data.Type.Length);
            config.MaxThreadIdWidth = Math.Max(config.MaxThreadIdWidth, node.Data.ThreadId.ToString().Length);
            if (node.Children.Count > 1)
                node.Children.Sort(_nodeComparer);

            if (node.Data.IsScope)
            {
                var endNode = node.Children.FirstOrDefault(c => c.Data.IsEndOfScope);
                if (endNode != null)
                {
                    node.DurationMs = endNode.Data.Timestamp - node.Data.Timestamp;
                    config.MaxDurWidth = Math.Max(config.MaxDurWidth, $"{node.DurationMs}ms".Length);
                }
            }
        }
        
        using var sw = new StreamWriter(outputPath, false, Encoding.UTF8);
        foreach (var root in roots)
        {
            RenderNode(sw, root, 0, config);
        }
    }

    private class LayoutConfig
    {
        public int MaxTypeWidth { get; set; } = 4;
        public int MaxDurWidth { get; set; } = 4;
        public int MaxThreadIdWidth { get; set; } = 3;
        public string TerminationText { get; set; } = "[!] TERMINATED_OR_CRASHED";
        public readonly static LayoutConfig Default = new();
    }

    private static void RenderNode(StreamWriter sw, ReconstructNode node, int indentLevel, LayoutConfig config)
    {
        var data = node.Data;

        var timePart = DateTimeOffset.FromUnixTimeMilliseconds(data.Timestamp).ToLocalTime().ToString(Const.FullTimeFormat);
        var threadPart = $"[T:{data.ThreadId.ToString($"D{config.MaxThreadIdWidth}")}]";
        var typePart = (data.IsEndOfScope ? "End" : data.Type).PadRight(config.MaxTypeWidth);
        var durStr = node.DurationMs >= 0 ? $"{node.DurationMs}ms" : "";
        var durPart = durStr.PadLeft(config.MaxDurWidth);

        var header = $"[{timePart}] {threadPart} [{typePart}] [{durPart}] | ";
        var headerWidth = header.Length;

        var indent = new string(' ', indentLevel * 2);
        var symbol = data.Type == "LogScope" ? "▼ " : (data.IsEndOfScope ? "└─ " : "├─ ");
        var contentOffset = indent.Length + symbol.Length;

        var summaryLines = (data.Summary ?? "").Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

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
        if (data.IsScope)
        {
            var orderedChildren = node.Children.OrderBy(c => c.Data.Id).ToList();
            foreach (var child in orderedChildren)
                RenderNode(sw, child, indentLevel + 1, config);

            if (orderedChildren.Count > 0 && !orderedChildren.Any(c => c.Data.IsEndOfScope))
                sw.WriteLine($"{new string(' ', headerWidth)}{indent}  {config.TerminationText}");
        }
    }
}
