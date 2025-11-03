using ElShrine.Common;
using System;
using System.Windows.Input;

namespace ElShrine.Wpf
{
    public class VMCommand(Action<object?> action, Func<object?, bool>? func, string translateName, Func<string, string>? translatedTextBuilder) : ICommand
    {
        public VMCommand(Action<object?> action, Func<object?, bool>? func = null, string translateName = Const.EmptyStr, string defaultTranslation = Const.EmptyStr) : this(action, func, translateName, tn => tn.Translate(defaultTranslation)) { }

        public event EventHandler? CanExecuteChanged;

        protected bool canExcuteResult = false;
        public bool CanExecute(object? parameter)
        {
            bool result;
            if (CanExecuteFunction is null) result = true;
            else result = CanExecuteFunction(parameter);
            if(canExcuteResult ^= result) CanExecuteChanged?.Invoke(this, new EventArgs());
            return result;
        }
        public void Execute(object? parameter)
        {
            if (ExecuteAction == null || !CanExecute(parameter)) return;
            ExecuteAction(parameter);
        }
        public void ForceExecute(object? parameter)
        {
            if (ExecuteAction == null) return;
            ExecuteAction(parameter);
        }

        public Action<object?>? ExecuteAction { get; init; } = action;
        public Func<object?, bool>? CanExecuteFunction { get; init; } = func;

        public Func<string, string>? TranslatedTextBuilder { get; init; } = translatedTextBuilder;
        public string TranslateName { get; init; } = translateName;
        private const string UntranslatedText = "##UNTRANSLATED##";
        public string TranslatedText => string.IsNullOrWhiteSpace(TranslateName) ? UntranslatedText : TranslatedTextBuilder?.Invoke(TranslateName) ?? UntranslatedText;
    }
}
