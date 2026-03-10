namespace ElShrine.Modules.Log;

/// <summary>
/// Represents a collection of multiple formatted text segments within a single log line.
/// Acts as a container for <see cref="LogItem"/> instances, allowing each segment to have its own style and format.
/// </summary>
/// <param name="LogItems">An array of <see cref="LogItem"/> that constitutes the line content.</param>
public sealed record EntryContent(LogItem[] LogItems)
{
    /// <summary>
    /// Creates a content instance from plain text, containing a single normal-format log item.
    /// </summary>
    /// <param name="normal">The plain text to display.</param>
    /// <returns>A new <see cref="EntryContent"/> instance wrapping the text.</returns>
    public static EntryContent FromText(string normal) => new(normal);

    /// <summary> Implicitly converts a string to an <see cref="EntryContent"/> with a normal log item. </summary>
    public static implicit operator EntryContent(string normal) => new([LogItem.Normal(normal)]);

    /// <summary> Implicitly converts a <see cref="LogItem"/> array to an <see cref="EntryContent"/>. </summary>
    public static implicit operator EntryContent(LogItem[] items) => new(items);

    /// <summary> Implicitly converts a single <see cref="LogItem"/> to an <see cref="EntryContent"/>. </summary>
    public static implicit operator EntryContent(LogItem item) => new([item]);

    /// <summary> Implicitly converts an <see cref="EntryContent"/> back to its underlying <see cref="LogItem"/> array. </summary>
    public static implicit operator LogItem[](EntryContent content) => content.LogItems;

    /// <summary>
    /// Concatenates all log items into a single string without additional delimiters.
    /// </summary>
    /// <returns>The combined string representation of all items.</returns>
    public override string ToString() => LogItems.BuildString(split: string.Empty);

    #region Table
    /// <summary>
    /// Helper method to build a tabular data representation as an <see cref="InfoEntry"/>.
    /// </summary>
    /// <param name="title">The title log item for the table.</param>
    /// <param name="entry">The output info entry containing the rendered table.</param>
    /// <param name="extraPad">Additional padding spaces between columns.</param>
    /// <param name="itemCols">A params array of column data, where each column is an array of <see cref="LogItem"/>.</param>
    public static void BuildTable(LogItem title, out EntryContent? entry, int extraPad = 1, params LogItem[][] itemCols)
    {
        entry = null;
        if (itemCols.Length == 0) return;

        var rowCount = itemCols.Max(c => c?.Length ?? 0);
        if (rowCount == 0) return;

        var colWidths = new int[itemCols.Length];
        for (var i = 0; i < itemCols.Length; i++)
            colWidths[i] = (itemCols[i]?.Max(i => i.ToString().Length) ?? 0) + extraPad;

        title = LogItem.Normal($"{title}\n", title.Style);
        var tableItems = new List<LogItem>();
        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < itemCols.Length; j++)
            {
                var col = itemCols[j];
                var targetWidth = colWidths[j];
                var isRowLastItem = j == itemCols.Length - 1;
                if (i < col.Length)
                {
                    var originalItem = col[i];
                    var text = originalItem.ToString().PadRight(targetWidth);
                    if (isRowLastItem) text += '\n';
                    var item = LogItem.Normal(text, originalItem.Style);
                    tableItems.Add(item);
                }
                else
                {
                    var text = new string(' ', targetWidth);
                    if (isRowLastItem) text += '\n';
                    tableItems.Add(LogItem.Normal(text));
                }
            }
        }
        entry = new([title, .. tableItems]);
    }
    public static LogItem[] OmitOrExecutingPattern(string text0, string text1, LogItemStyle text1Style, LogItemStyle text0Style = LogItemStyle.Info)
        => [LogItem.Normal(text0, text0Style), LogItem.Normal(text1, text1Style), LogItem.Normal("...", text0Style)];
    #endregion
}