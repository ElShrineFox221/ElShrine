using ElShrine.Modules;
using ElShrine.Wpf.Converters;
using System.Windows.Controls;

namespace ElShrine.VisualTool.Pages.OptionsHub.OptionItemEditors
{
    [OptionItemEditor]
    public partial class BoolEditor : UserControl, IOptionItemEditor
    {
        public BoolEditor()
        {
            InitializeComponent();
        }

        public void RegisterUpdateValue(OptionItem source, Action<object?> updatedValueFromUI, out Action<object?> updateValueToUI)
        {
            PART_TOGBTN.IsChecked = source.GetValue() is bool b && b;
            PART_TOGBTN.Unchecked += (s, e) => updatedValueFromUI(false);
            PART_TOGBTN.Checked += (s, e) => updatedValueFromUI(true);
            updateValueToUI = (v) =>
            {
                var b = CommonConverter.ToBool(v);
                if (b ^ PART_TOGBTN.IsChecked ?? false) PART_TOGBTN.IsChecked = b;
            };
        }
    }
}
