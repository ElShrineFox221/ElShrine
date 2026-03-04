namespace ElShrine.Modules.Log;

/// <summary>
/// A delegate that defines how to build additional log content when a scope ends.
/// </summary>
/// <param name="accessor">The read-only accessor of the closing scope.</param>
/// <returns>The constructed <see cref="EntryContent"/> to be displayed.</returns>
public delegate EntryContent EndItemsBuilder(IScopeAccessor accessor);

/// <summary>
/// Configures the visual feedback and summary items displayed when a <see cref="LogScope"/> is concluded.
/// </summary>
/// <param name="ShowSuc">Determines whether to show a "Completed" header upon successful completion.</param>
/// <param name="ShowError">Determines whether to show a "Failed" header if the scope contains errors.</param>
/// <param name="ItemsBuilder">A custom builder to append additional information to the end of the scope.</param>
public record EndConfiguration(bool ShowSuc = true, bool ShowError = true, EndItemsBuilder? ItemsBuilder = null)
{
    /// <summary>
    /// Gets the default configuration: shows both success and error headers with no custom builder.
    /// </summary>
    public readonly static EndConfiguration Default = new();

    /// <summary>
    /// Initializes a new instance of <see cref="EndConfiguration"/> with a simple text summary.
    /// </summary>
    /// <param name="text">The summary text to display.</param>
    /// <param name="showSuc">Whether to show success indicators.</param>
    /// <param name="showError">Whether to show error indicators.</param>
    public EndConfiguration(string text, bool showSuc = true, bool showError = true)
        : this(showSuc, showError, s => LogItem.Normal(text)) { }
}