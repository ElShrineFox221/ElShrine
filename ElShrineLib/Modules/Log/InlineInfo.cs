namespace ElShrine.Modules.Log;

public sealed record InlineInfo(LogItem[] Items)
{
    public static InlineInfo FromText(string normal) => new(normal);
    public static implicit operator InlineInfo(string normal) => new([LogItem.Normal(normal)]);
    public static implicit operator InlineInfo(LogItem[] items) => new(items);
    public static implicit operator InlineInfo(LogItem item) => new([item]);
    public static implicit operator LogItem[](InlineInfo inlineInfo) => inlineInfo.Items;
    public override string ToString() => Items.BuildString(split: string.Empty);
    public IEnumerable<(string text, LogItemStyle style, LogItemFormat format)> GetStyledSegments()
        => Items.Select(i => (i.Text, i.Style, i.Format));
}
