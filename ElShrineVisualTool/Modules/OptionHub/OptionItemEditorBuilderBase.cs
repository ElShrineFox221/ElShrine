using ElShrine.EOption;
using System.Reflection;
using System.Windows;

namespace ElShrine.Modules.OptionHub
{
    public abstract class OptionItemEditorBuilderBase
    {
        public readonly static Dictionary<Type, MethodInfo> BuilderMethodsDictionary = [];
        static OptionItemEditorBuilderBase()
        {
            var classes = typeof(OptionItemEditorBuilderBase).GetImplements();
            foreach (var type in classes)
            {
                var instance = Activator.CreateInstance(type) as OptionItemEditorBuilderBase;
                BuilderMethodsDictionary.Add(instance?.ValueType ?? throw new("Failed to create instance."), type.GetMethod(nameof(BuildContorl), BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new("Found no builder method."));
            }
        }
        protected abstract Type ValueType { get; }
        protected abstract FrameworkElement BuildContorl(OptionItem optionItem, Action<object?> updateAction);
    }
}
