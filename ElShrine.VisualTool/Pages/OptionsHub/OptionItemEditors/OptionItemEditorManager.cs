using ElShrine.Modules;
using ElShrine.Wpf;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace ElShrine.VisualTool.Pages.OptionsHub.OptionItemEditors
{
    public sealed record OptionItemEditorKey(string CataName, string ItemName, Type ValueType);
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class OptionItemEditorAttribute() : ValidatableClassAttribute
    {
        protected override bool ValidateType(Type typeToValidate)
        {
            if(typeToValidate.IsImplementOf(typeof(IOptionItemEditor))) return true;
            ValidateFailedReason = $"Type <{typeToValidate.FullName}> is not a valid option item editor, it should be implement of <{typeof(IOptionItemEditor).FullName}>.";
            return false;
        }
    }

    public sealed class TarInfo : Freezable
    {
        public static readonly DependencyProperty CataNameProperty = DependencyProperty.Register(nameof(CataName), typeof(string), typeof(TarInfo));
        public static readonly DependencyProperty ItemNameProperty = DependencyProperty.Register(nameof(ItemName), typeof(string), typeof(TarInfo));
        public string CataName
        {
            get => (string)GetValue(CataNameProperty);
            set => SetValue(CataNameProperty, value);
        }
        public string ItemName
        {
            get => (string)GetValue(ItemNameProperty);
            set => SetValue(ItemNameProperty, value);
        }

        public TarInfo() { }

        protected override Freezable CreateInstanceCore() => new TarInfo();
    }
    public sealed class TarInfoCollection : FreezableCollection<TarInfo> { }
    [InitializationInfo(PreInstantiate = true)]
    public static class OptionItemEditorManager
    {
        #region DPs
        public static readonly DependencyProperty TarTypeProperty = DependencyProperty.RegisterAttached(
            nameof(TarTypeProperty).ToPropRegName(),
            typeof(Type),
            typeof(OptionItemEditorManager),
            new FrameworkPropertyMetadata(
                defaultValue: null,
                propertyChangedCallback: OnTarTypeChanged
        ));
        public static readonly DependencyProperty TarInfosProperty = DependencyProperty.RegisterAttached(
            nameof(TarInfosProperty).ToPropRegName(),
            typeof(TarInfoCollection),
            typeof(OptionItemEditorManager),
            new FrameworkPropertyMetadata(
                defaultValue: null,
                propertyChangedCallback: OnTarInfosChanged
        ));
        #endregion

        #region dp methods
        public static Type GetTarType(DependencyObject d) => (Type)d.GetValue(TarTypeProperty);
        public static void SetTarType(DependencyObject d, Type value) => d.SetValue(TarTypeProperty, value);
        public static TarInfoCollection GetTarInfos(DependencyObject d) => (TarInfoCollection)d.GetValue(TarInfosProperty);
        public static void SetTarInfos(DependencyObject d, TarInfoCollection value) => d.SetValue(TarInfosProperty, value);
        #endregion

        #region callbacks

        private static void OnTarTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UserControl) 
            {
                var editorType = d.GetType();
                if (e.NewValue is Type newType) EditorByValueType[newType] = editorType;
                if (e.OldValue is Type oldType) EditorByValueType.Remove(oldType);
            }
        }
        private static void OnTarInfosChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UserControl) 
            {
                var editorType = d.GetType();
                if (d.GetValue(TarTypeProperty) is not Type newType) return;
                if (e.OldValue is TarInfoCollection oldInfos)
                {
                    foreach (var info in oldInfos)
                    {
                        var key = new OptionItemEditorKey(info.CataName, info.ItemName, newType);
                        EditorByPrioKey.Remove(key);
                    }
                }
                if (e.NewValue is TarInfoCollection newInfos)
                {
                    foreach (var info in newInfos)
                    {
                        var key = new OptionItemEditorKey(info.CataName, info.ItemName, newType);
                        EditorByPrioKey[key] = editorType;
                    }
                }
            }
        }
        #endregion

        static OptionItemEditorManager()
        {
            RecollectOptionItemEditorBuilders(null);
            ClassesManager.Instance.AssembliesUpdated += RecollectOptionItemEditorBuilders;
        }

        private static readonly Dictionary<OptionItemEditorKey, Type> EditorByPrioKey = [];
        private static readonly Dictionary<Type, Type> EditorByValueType = [];
        private static void RecollectOptionItemEditorBuilders(Assembly[]? assemblies)
        {
            var cm = ClassesManager.Instance;
            var attributedClasses = cm.GetClassesByAttribute<OptionItemEditorAttribute>(range: assemblies);
            foreach (var (editorType, _) in attributedClasses)
            {
                _ = Activator.CreateInstance(editorType);
            }
        }

        public static UserControl? GetOptionItemEditor(OptionItem item, Action<object?> updatedValueFromUI, out Action<object?>? updateValueToUI)
        {
            var control = GetOptionItemEditorInternal(item);
            updateValueToUI = null;
            if (control is IOptionItemEditor editor)
            {
                editor.RegisterUpdateValue(item, updatedValueFromUI, out updateValueToUI);
            }
            return control;
        }
        private static UserControl? GetOptionItemEditorInternal(OptionItem item)
        {
            if (EditorByPrioKey.TryGetValue(new OptionItemEditorKey(item.VirtualCataName, item.VirtualItemName, item.ValueType), out var editorType))
                return (UserControl?)Activator.CreateInstance(editorType);
            if (EditorByValueType.TryGetValue(item.ValueType, out editorType))
                return (UserControl?)Activator.CreateInstance(editorType);
            return null;
        }
    }
    public interface IOptionItemEditor
    {
        void RegisterUpdateValue(OptionItem source, Action<object?> updatedValueFromUI, out Action<object?> updateValueToUI);
    }
}
