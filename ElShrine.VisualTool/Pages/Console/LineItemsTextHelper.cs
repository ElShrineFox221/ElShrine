using ElShrine.Modules.Log;
using ElShrine.VisualTool.Pages.Console.ViewModel;
using ElShrine.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ElShrine.VisualTool.Pages.Console;

public static class LineItemsTextHelper
{
    public static readonly DependencyProperty LineItemsProperty =
        DependencyProperty.RegisterAttached(
            nameof(LineItemsProperty).ToPropRegName(),
            typeof(IEnumerable<LogItem>),
            typeof(LineItemsTextHelper),
            new PropertyMetadata(null, OnLineItemsChanged));

    public static IEnumerable<LogItem> GetLineItems(DependencyObject obj)
        => (IEnumerable<LogItem>) obj.GetValue(LineItemsProperty);

    public static void SetLineItems(DependencyObject obj, IEnumerable<LogItem> value)
        => obj.SetValue(LineItemsProperty, value);

    private static void OnLineItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBlock textBlock)
        {
            textBlock.Inlines.Clear();
            if (e.NewValue is not IEnumerable<LogItem> items) 
                return;
            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item.Text)) 
                    continue;
                var textParts = item.ToString().Split(["\r\n", "\n"], StringSplitOptions.None);
                for (int i = 0; i < textParts.Length; i++)
                {
                    if (!string.IsNullOrEmpty(textParts[i]))
                    {
                        var run = new Run(textParts[i])
                        {
                            Foreground = LineItemVM.PaintModeToFontColor(ConsoleVM.Instance.Option.BackColor, item.Style).ToSolidBrush()
                        };
                        textBlock.Inlines.Add(run);
                    }

                    if (i < textParts.Length - 1)
                        textBlock.Inlines.Add(new LineBreak());
                }
            }
        }
    }
}
