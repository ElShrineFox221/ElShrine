using ElShrine.EOption;
using ElShrine.Modules.OptionHub.ViewModel;
using ElShrine.Wpf.Controls;
using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls;

namespace ElShrine.Modules.OptionHub
{
    public class BoolEditorBuilder : OptionItemEditorBuilderBase
    {
        protected override Type ValueType => typeof(bool);
        protected override FrameworkElement BuildContorl(OptionItem optionItem, Action<object?> updateAction)
        {
            var toggle = new EToggleButton
            {
                IsChecked = (bool)(optionItem.Value ?? false),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            toggle.Checked += (_, _) => updateAction.Invoke(true);
            toggle.Unchecked += (_, _) => updateAction.Invoke(false);
            return toggle;
        }
    }
    public class EnumEditorBuilder : OptionItemEditorBuilderBase
    {
        protected override Type ValueType => typeof(LoadMode);
        protected override FrameworkElement BuildContorl(OptionItem optionItem, Action<object?> updateAction)
        {
            var combo = new EComboBox
            {
                ItemsSource = Enum.GetValues(optionItem.ValueType),
                SelectedValue = optionItem.Value,
            };
            combo.SelectionChanged += (_, _) => updateAction.Invoke(combo.SelectedValue);
            return combo;
        }
    }
    public class StringEditorBuilder : OptionItemEditorBuilderBase
    {
        protected override Type ValueType => typeof(string);
        protected override FrameworkElement BuildContorl(OptionItem optionItem, Action<object?> updateAction)
        {
            var textBox = new ETextBox
            {
                Text = optionItem.Value?.ToString() ?? string.Empty
            };
            textBox.TextChanged += (_, _) => updateAction.Invoke(textBox.Text);
            return textBox;
        }
    }
    public class ThemeEditorBuilder : OptionItemEditorBuilderBase
    {
        protected override Type ValueType => typeof(Theme);
        protected override FrameworkElement BuildContorl(OptionItem optionItem, Action<object?> updateAction)
        {
            ResourceDictionary resourceDict = new()
            {
                Source = new Uri(ThemeEditorVM.DataTemplateUri, UriKind.RelativeOrAbsolute)
            };
            var currentAppDics = Application.Current.Resources.MergedDictionaries;
            if (!currentAppDics.Any((ResourceDictionary rd) => rd.Source.OriginalString == resourceDict.Source.OriginalString)) Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            var dataTemplate = resourceDict[ThemeEditorVM.DataTemplateName] as DataTemplate;
            ContentControl contentControl = new()
            {
                Content = new ThemeEditorVM(),
                ContentTemplate = dataTemplate,
            };
            return contentControl;
        }
    }
}
