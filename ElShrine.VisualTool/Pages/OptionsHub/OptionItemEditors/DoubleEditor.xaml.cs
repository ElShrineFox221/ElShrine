using ElShrine.Modules;
using ElShrine.Wpf.Converters;
using System.Windows.Controls;

namespace ElShrine.VisualTool.Pages.OptionsHub.OptionItemEditors
{
    [OptionItemEditor]
    public partial class DoubleEditor : UserControl, IOptionItemEditor
    {
        public DoubleEditor()
        {
            InitializeComponent();
        }
        public void RegisterUpdateValue(OptionItem source, Action<object?> updatedValueFromUI, out Action<object?> updateValueToUI)
        {
            PART_DoubleInput.Value = CommonConverter.ToNumber(source.GetValue());
            PART_DoubleInput.ValueChanged += (s, e) =>
            {
                updatedValueFromUI(e.NewValue);
            };
            updateValueToUI = (v) =>
            {
                var num = CommonConverter.ToNumber(v);
                if (PART_DoubleInput.Value != num) PART_DoubleInput.Value = num;
            };
        }
    }
}
