using ElShrine.Modules.Option;
using ElShrine.VisualTool.Pages.OptionsHub.OptionItemEditors;
using ElShrine.Wpf;
using System.Windows;

namespace ElShrine.VisualTool.Pages.OptionsHub
{
    public sealed class OptionItemVM : ViewModelBase<OptionItem>
    {
        public string ItemName { get; init; }
        public string CataName { get; init; }
        public string ActualName { get; init; }
        public string ClassName { get; init; }
        public string Description { get; init; }

        public object? Value
        {
            get => Model.GetValue();
            private set
            {
                if (Equals(Value, value)) return;
                Model.SetValue(value);
                NotifyPropertiesChanged(nameof(Value));
            }
        }
        public object? CachedNewValue
        {
            get => field;
            private set
            {
                if (Equals(field, value)) return;
                field = value;
                UpdateValueToUI?.Invoke(field);
                NotifyPropertiesChanged(nameof(CachedNewValue));
            }
        }
        public object? OldValue
        {
            get => field;
            private set
            {
                if (Equals(field, value)) return;
                field = value;
                NotifyPropertiesChanged(nameof(OldValue));
            }
        }
        public FrameworkElement? Control { get; init; }
        private readonly Action<object?>? UpdateValueToUI;

        public OptionItemVM(OptionItem model, Action<OptionItemVM, object?> updatedValueFromUI) : base(model)
        {
            ItemName = model.VirtualItemName;
            CataName = model.VirtualCataName;
            ActualName = model.ActualItemName;
            ClassName = model.ActualCataName;
            Description = model.Description;
            CachedNewValue = model.GetValue();
            OldValue = model.GetValue();
            //
            Control = OptionItemEditorManager.GetOptionItemEditor(Model, v =>
            {
                updatedValueFromUI(this, v);
                SetValue(v);
            }, out var updateValueToUI);
            UpdateValueToUI = updateValueToUI;
        }

        public void Cancel()
        {
            CachedNewValue = OldValue;
            Value = OldValue;
        }
        public void Confrim()
        {
            if (!isPreviewModeEnabled) Value = CachedNewValue;
            OldValue = CachedNewValue;
        }
        private bool isPreviewModeEnabled = false;
        public void RefreshPreviewModeInfo(bool enabled)
        {
            isPreviewModeEnabled = enabled;
            if (isPreviewModeEnabled) Value = CachedNewValue;
            else Value = OldValue;
        }
        public void SetValue(object? value)
        {
            CachedNewValue = value;
            if (isPreviewModeEnabled) Value = CachedNewValue;
        }
    }
}
