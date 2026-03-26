using ElShrine.Modules.Option;
using ElShrine.Wpf.Converters;
using System.Windows.Controls;

namespace ElShrine.VisualTool.Pages.OptionsHub.OptionItemEditors
{
    [OptionItemEditor]
    public partial class IntEditor : UserControl, IOptionItemEditor
    {
        public IntEditor()
        {
            InitializeComponent();
        }

        public void RegisterUpdateValue(OptionItem source, Action<object?> updatedValueFromUI, out Action<object?> updateValueToUI)
        {
            PART_IntInput.Value = source.GetValue() is int i ? i : 0;
            PART_IntInput.ValueChanged += (s, e) =>
            {
                updatedValueFromUI((int)e.NewValue);
            };
            updateValueToUI = (v) =>
            {
                var num = CommonConverter.ToNumber(v);
                if (PART_IntInput.Value != num) PART_IntInput.Value = (int)num;
            };
        }
    }
}
