using System;
using System.Collections.Generic;
using System.Windows.Markup;

namespace ElShrine.Wpf
{
    public class EnumBindingSourceExtension(Type? enumType) : MarkupExtension
    {
        public EnumBindingSourceExtension() : this(null) { }
        public Type? EnumType { get; init; } = (enumType?.IsEnum ?? false) ? enumType : null;
        public override object ProvideValue(IServiceProvider serviceProvider) => EnumType is null ? new List<object>() : Enum.GetValues(EnumType);
    }
}
