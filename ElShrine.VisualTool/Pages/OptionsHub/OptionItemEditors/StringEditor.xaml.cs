using ElShrine.Modules;
using System.Windows.Controls;

namespace ElShrine.VisualTool.Pages.OptionsHub.OptionItemEditors
{
    [OptionItemEditor]
    public partial class StringEditor : UserControl, IOptionItemEditor
    {
        public StringEditor()
        {
            InitializeComponent();
        }
        public void RegisterUpdateValue(OptionItem source, Action<object?> updatedValueFromUI, out Action<object?> updateValueToUI)
        {
            PART_PathInput.Text = source.GetValue()?.ToString() ?? string.Empty;
            PART_PathInput.TextChanged += (s, e) =>
            {
                updatedValueFromUI(PART_PathInput.Text);
            };
            updateValueToUI = (v) =>
            {
                var str = v?.ToString() ?? string.Empty;
                if (PART_PathInput.Text != str) PART_PathInput.Text = str;
            };
        }
    }
}
