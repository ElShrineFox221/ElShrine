using System.Collections.Generic;
using System.Reflection;
using DataHandler = ElShrine.EFile.DataHandler;

namespace ElShrine.Old.Wpf
{
    public partial class Option
    {
        public class OptionOverwriter<T> where T : Option
        {
            public readonly List<string> Exceptions = [];
            private class OptionsValue
            {
                public OptionsValue() { }
                public string Name { get; set; } = string.Empty;
                public object? Value { get; set; } = null;
            }
            public static OptionOverwriter<TOption> OverrideValues<TOption>(TOption dataProvider)
                where TOption : Option
            {
                //create DataDeliverer
                OptionOverwriter<TOption> overwriter = new();
                //get Values
                List<OptionsValue> OptionsNameValue = [];
                PropertyInfo[] properties = typeof(TOption).GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (PropertyInfo property in properties) OptionsNameValue.Add(new() { Name = property.Name, Value = property.GetValue(dataProvider) });
                //get DataReceiver
                T t = (T)GetInstance();
                //Receive | Overwrite
                foreach (OptionsValue optionsValue in OptionsNameValue)
                {
                    PropertyInfo? info = typeof(T).GetProperty(optionsValue.Name);
                    object? o = optionsValue.Value;
                    if (info == null) continue;
                    try { info?.SetValue(t, o); }
                    catch { overwriter.Exceptions.Add($"Can not set a value: {o}, type: {info.PropertyType}, name: {info.Name}"); }
                }

                return overwriter;
            }
            protected OptionOverwriter() { }
        }
        public static void Save() 
            => DataHandler.Write(GetInstance());
        public static OptionOverwriter<Option> Load()
        {
            var result = DataHandler.Read<Option>();
            var overResult = OptionOverwriter<Option>.OverrideValues(result.Data ?? throw new("Failed to override options."));
            return overResult;
        }
    }
}
