namespace ElShrine.Modules.Log;

/// <summary>
/// Defines the visual color styles for log text items.
/// </summary>
public enum LogItemStyle
{
    /// <summary> Default information (typically white or standard text). </summary>
    Info,
    /// <summary> Secondary or auxiliary information (typically gray). </summary>
    SubInfo,
    /// <summary> Warning messages (typically yellow). </summary>
    Warning,
    /// <summary> Error messages (typically red). </summary>
    Error,
    /// <summary> Success messages (typically green). </summary>
    Success,
    /// <summary> Blue notification style. </summary>
    NoticeBlue,
    /// <summary> Purple notification style. </summary>
    NoticePurple,
    /// <summary> Cyan notification style. </summary>
    NoticeCyan,
    /// <summary> Dark yellow notification style. </summary>
    NoticeDarkYellow,
    /// <summary> Pale green notification style. </summary>
    NoticePaleGreen,
}

/// <summary>
/// Defines the wrapping format or delimiters for log text.
/// </summary>
public enum LogItemFormat
{
    /// <summary> Plain text without any wrapping: text </summary>
    Normal = 0,
    /// <summary> Square brackets: [text] </summary>
    Header,
    /// <summary> Parentheses: (text) </summary>
    Bracket,
    /// <summary> Curly braces: {text} </summary>
    BBracket,
    /// <summary> Quotation marks: "text" </summary>
    Quote,
}

/// <summary>
/// Represents a single unit of a log entry, containing text content, style, format, and translucency state.
/// </summary>
/// <param name="Text">The string content to be displayed.</param>
/// <param name="Style">The color style used for rendering.</param>
/// <param name="Format">The wrapping format used to enclose the text.</param>
/// <param name="IsTranslucent">Indicates whether the item should be rendered with partial transparency.</param>
public readonly record struct LogItem(string Text, LogItemStyle Style, LogItemFormat Format, bool IsTranslucent = false)
{
    /// <summary>
    /// Creates a log item with no special formatting.
    /// </summary>
    /// <param name="text">The log text.</param>
    /// <param name="style">The color style. Defaults to <see cref="LogItemStyle.Info"/>.</param>
    /// <param name="isTranslucent">Whether the text is translucent.</param>
    /// <returns>A new <see cref="LogItem"/> with normal format.</returns>
    public static LogItem Normal(string text, LogItemStyle style = LogItemStyle.Info, bool isTranslucent = false)
        => new(text, style, LogItemFormat.Normal, isTranslucent);

    /// <summary>
    /// Creates a log item wrapped in square brackets: [text].
    /// </summary>
    /// <param name="header">The header text.</param>
    /// <param name="style">The color style.</param>
    /// <param name="isTranslucent">Whether the text is translucent.</param>
    /// <returns>A new <see cref="LogItem"/> with header format.</returns>
    public static LogItem Header(string header, LogItemStyle style = LogItemStyle.Info, bool isTranslucent = false)
        => new(header, style, LogItemFormat.Header, isTranslucent);

    /// <summary>
    /// Creates a log item wrapped in parentheses: (text).
    /// </summary>
    /// <param name="text">The log text.</param>
    /// <param name="style">The color style.</param>
    /// <param name="isTranslucent">Whether the text is translucent.</param>
    /// <returns>A new <see cref="LogItem"/> with bracket format.</returns>
    public static LogItem Bracket(string text, LogItemStyle style = LogItemStyle.Info, bool isTranslucent = false)
        => new(text, style, LogItemFormat.Bracket, isTranslucent);

    /// <summary>
    /// Creates a log item wrapped in curly braces: {text}.
    /// </summary>
    /// <param name="text">The log text.</param>
    /// <param name="style">The color style.</param>
    /// <param name="isTranslucent">Whether the text is translucent.</param>
    /// <returns>A new <see cref="LogItem"/> with curly bracket format.</returns>
    public static LogItem CurlyBracket(string text, LogItemStyle style = LogItemStyle.Info, bool isTranslucent = false)
        => new(text, style, LogItemFormat.BBracket, isTranslucent);

    /// <summary>
    /// Creates a log item wrapped in quotation marks: "text".
    /// </summary>
    /// <param name="text">The log text.</param>
    /// <param name="style">The color style.</param>
    /// <param name="isTranslucent">Whether the text is translucent.</param>
    /// <returns>A new <see cref="LogItem"/> with quote format.</returns>
    public static LogItem Quote(string text, LogItemStyle style = LogItemStyle.Info, bool isTranslucent = false)
        => new(text, style, LogItemFormat.Quote, isTranslucent);

    /// <summary>
    /// Creates an empty log item with default settings.
    /// </summary>
    /// <returns>An empty <see cref="LogItem"/>.</returns>
    public static LogItem Empty()
        => new(string.Empty, LogItemStyle.Info, LogItemFormat.Normal, false);

    /// <summary>
    /// Returns the formatted string based on the defined <see cref="Format"/>.
    /// </summary>
    /// <returns>A string representation of the log item with its delimiters.</returns>
    public override string ToString()
    {
        return Format switch
        {
            LogItemFormat.Normal => Text,
            LogItemFormat.Header => $"[{Text}]",
            LogItemFormat.Bracket => $"({Text})",
            LogItemFormat.BBracket => $"{{{Text}}}",
            LogItemFormat.Quote => $"\"{Text}\"",
            _ => Text
        };
    }
}