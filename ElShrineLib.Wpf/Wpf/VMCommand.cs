using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ElShrine.Wpf
{
    public class VMCommand : ViewModelBase, ICommand
    {
        public VMCommand(Action<object?> executeAction, Func<object?, bool>? canExecuteFunc, string translateName, Func<string, string>? translatedTextBuilder)
        {
            ExecuteAction = executeAction;
            CanExecuteFunction = canExecuteFunc;
            TranslatedTextBuilder = translatedTextBuilder;
            TranslateName = translateName;
        }
        public VMCommand(Action<object?> executeAction, Func<object?, bool>? canExecuteFunc = null, string translateName = Const.EmptyStr, string defaultTranslation = Const.EmptyStr) : this(executeAction, canExecuteFunc, translateName, tn => tn.Translate(defaultTranslation)) { }
        public VMCommand(Func<object?, Task> executeAsyncAction, Func<object?, bool>? canExecuteFunc, string translateName, Func<string, string>? translatedTextBuilder)
        {
            var synchronizationContext = SynchronizationContext.Current;
            ExecuteAction = o =>
            {
                if (IsRunning) return;
                var task = executeAsyncAction(o);
                RunningTask = task;
                task.ContinueWith(t =>
                {
                    //LOG if (t.Exception is not null) EConsole.ConsoleManager.ListErrorInfo(t.Exception);
                    if (synchronizationContext != null) synchronizationContext.Post(_ => RunningTask = null, null);
                    else RunningTask = null;
                }, TaskScheduler.Default);
            };
            CanExecuteFunction = canExecuteFunc;
            TranslatedTextBuilder = translatedTextBuilder;
            TranslateName = translateName;
        }
        public VMCommand(Func<object?, Task> executeAsyncAction, Func<object?, bool>? canExecuteFunc = null, string translateName = Const.EmptyStr, string defaultTranslation = Const.EmptyStr)
            : this(executeAsyncAction, canExecuteFunc, translateName, tn => tn.Translate(defaultTranslation))
        { }


        public event EventHandler? CanExecuteChanged;

        protected bool canExcuteResult = false;
        public bool CanExecute(object? parameter)
        {
            bool result;
            if (CanExecuteFunction is null) result = true;
            else result = CanExecuteFunction(parameter);
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

        public void InvokeSync(object? parameter, Dispatcher? dispatcher = null)
        {
            dispatcher ??= Application.Current.Dispatcher;
            Task? runningTask = null;
            dispatcher.Invoke(() =>
            {
                //LOG ListContentInfo("Execute action on ui dispatcher");
                Execute(parameter);
                runningTask = RunningTask;
            });
            if (runningTask is not null)
            {
                //LOG ListContentInfo("Async action will blocks the thread...");
                try
                {
                    runningTask.Wait();
                }
                finally
                {
                    //LOG ListContentInfo("Async action completed.");
                }
            }
            //LOG else ListContentInfo("Sync action wont blocks the thread.");
        }

        public Action<object?>? ExecuteAction { get; init; }
        public Func<object?, bool>? CanExecuteFunction { get; init; }

        public Func<string, string>? TranslatedTextBuilder { get; init; }
        public string TranslateName { get; init; }
        private const string UntranslatedText = "##UNTRANSLATED##";
        public string TranslatedText => string.IsNullOrWhiteSpace(TranslateName) ? UntranslatedText : TranslatedTextBuilder?.Invoke(TranslateName) ?? UntranslatedText;

        #region VMCTask
        private Task? runningTask = null;
        public Task? RunningTask
        {
            get => runningTask;
            set
            {
                if (!IsRunning)
                {
                    runningTask = value;
                    CanExecuteChanged?.Invoke(this, new EventArgs());
                    NotifyPropertyChanged(nameof(RunningTask), nameof(IsRunning));
                }
            }
        }
        public bool IsRunning => RunningTask != null && !RunningTask.IsCompleted;
        #endregion
    }
}
