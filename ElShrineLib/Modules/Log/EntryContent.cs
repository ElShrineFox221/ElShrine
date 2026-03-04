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
}