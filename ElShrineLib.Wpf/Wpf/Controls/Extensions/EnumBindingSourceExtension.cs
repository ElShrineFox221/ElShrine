using System;
using System.Windows.Markup;

namespace ElShrine.Wpf.Controls.Extensions
{
    public class EnumBindingSourceExtension(Type enumType) : MarkupExtension
    {
        public Type EnumType { get; init; } = enumType.IsEnum ? enumType : throw new ArgumentException("EnumType must be a non-nullable Enum type.");
        public override object ProvideValue(IServiceProvider serviceProvider) => Enum.GetValues(EnumType);
    }
}
