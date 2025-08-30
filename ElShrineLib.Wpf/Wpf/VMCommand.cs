using System;
using System.Windows.Input;

namespace ElShrine.Wpf
{
    public class VMCommand(Action<object?> action, Func<object?, bool>? func) : ICommand
    {
        public VMCommand(Action<object?> action) : this(action, null) { }

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
            if (ExecuteAction == null) return;
            ExecuteAction(parameter);
        }

        public Action<object?>? ExecuteAction { get; set; } = action;
        public Func<object?, bool>? CanExecuteFunction { get; set; } = func;
    }
}
