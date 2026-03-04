namespace ElShrine.Modules.Log;

/// <summary>
/// 定义日志文本的颜色样式
/// </summary>
public enum LogItemStyle
{
    /// <summary> 默认信息（白色/常规） </summary>
    Info,
    /// <summary> 次要信息（灰色） </summary>
    SubInfo,
    /// <summary> 警告（黄色） </summary>
    Warning,
    /// <summary> 错误（红色） </summary>
    Error,
    /// <summary> 成功（绿色） </summary>
    Success,
    /// <summary> 蓝色通知 </summary>
    NoticeBlue,
    /// <summary> 紫色通知 </summary>
    NoticePurple,
    /// <summary> 青色通知 </summary>
    NoticeCyan,
    /// <summary> 暗黄色通知 </summary>
    NoticeDarkYellow,
    /// <summary> 浅绿色通知 </summary>
    NoticePaleGreen,
}

/// <summary>
/// 定义日志文本的包装格式
/// </summary>
public enum LogItemFormat
{
    /// <summary> 纯文本: text </summary>
    Normal = 0,
    /// <summary> 方括号: [text] </summary>
    Header,
    /// <summary> 圆括号: (text) </summary>
    Bracket,
    /// <summary> 花括号: {text} </summary>
    BBracket,
    /// <summary> 引号: "text" </summary>
    Quote,
}

/// <summary>
/// 表示一个日志条目单元，包含文本内容、样式、格式以及透明度状态
/// </summary>
/// <param name="Text">显示文本</param>
/// <param name="Style">颜色样式</param>
/// <param name="Format">包装格式</param>
/// <param name="IsTranslucent">是否半透明显示</param>
public readonly record struct LogItem(string Text, LogItemStyle Style, LogItemFormat Format, bool IsTranslucent = false)
{
    /// <summary>
    /// 创建一个普通格式的日志条目
    /// </summary>
    public static LogItem Normal(string text, LogItemStyle style = LogItemStyle.Info, bool isTranslucent = false)
        => new(text, style, LogItemFormat.Normal, isTranslucent);

    /// <summary>
    /// 创建一个带方括号的标题条目: [text]
    /// </summary>
    public static LogItem Header(string header, LogItemStyle style = LogItemStyle.Info, bool isTranslucent = false)
        => new(header, style, LogItemFormat.Header, isTranslucent);

    /// <summary>
    /// 创建一个带圆括号的条目: (text)
    /// </summary>
    public static LogItem Bracket(string text, LogItemStyle style = LogItemStyle.Info, bool isTranslucent = false)
        => new(text, style, LogItemFormat.Bracket, isTranslucent);

    /// <summary>
    /// 创建一个带花括号的条目: {text}
    /// </summary>
    public static LogItem CurlyBracket(string text, LogItemStyle style = LogItemStyle.Info, bool isTranslucent = false)
        => new(text, style, LogItemFormat.BBracket, isTranslucent);

    /// <summary>
    /// 创建一个带引号的条目: "text"
    /// </summary>
    public static LogItem Quote(string text, LogItemStyle style = LogItemStyle.Info, bool isTranslucent = false)
        => new(text, style, LogItemFormat.Quote, isTranslucent);

    /// <summary>
    /// 创建一个空内容的条目
    /// </summary>
    public static LogItem Empty()
        => new(string.Empty, LogItemStyle.Info, LogItemFormat.Normal, false);

    /// <summary>
    /// 根据定义的 <see cref="Format"/> 返回格式化后的字符串
    /// </summary>
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