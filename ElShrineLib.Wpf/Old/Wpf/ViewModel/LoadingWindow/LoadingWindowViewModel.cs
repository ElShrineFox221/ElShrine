using ElShrine.Old.Wpf.Controls;
using ElShrine.Wpf;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ElShrine.Old.Wpf.ViewModel.LoadingWindow
{
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public sealed class LoadingWindowViewModel : EWindowViewModelBase
    {
        public static class LoadingWindowStyle
        {
            public const string Base = "ElShrineLoadingWindow_BaseStyle";
            public const string Console = "ElShrineLoadingWindow_ConsoleStyle";
        }
        private LoadingWindowViewModel(Window ownerWindow) : base(ownerWindow)
        {

        }

        private static LoadingWindowViewModel? instance = null;
        public static LoadingWindowViewModel Instance => instance ??= SetWindowViewModel<LoadingWindowViewModel>(new()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
        });
        public static LoadingWindowViewModel CreateInstanceIsolated()
            => new(new());

        public static void Loading(TimeSpan? Span = null, Brush? Background = null, string StyleName = LoadingWindowStyle.Base, Action? Act = null)
        {
            Window window = Instance.ownerWindow ?? new();

            Style? style = (Style?)new Window().TryFindResource(StyleName);
            if (style is not null) window.Style = style;
            TimeSpan timeSpan = Span ?? TimeSpan.FromMilliseconds(3000);
            Action act;
            if (Act is null) act = () =>
            {
                Methods.SimpleDoubleAnimation(new(TimeSpan.FromMilliseconds(300)), 0, null, null, window, UIElement.OpacityProperty);
            };
            else act = Act;
            Task.Run(() =>
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(1000));
                Task.Run(() =>
                {
                    window.Dispatcher.Invoke(() => ElShrineLogoContent.DrawLogos(new(timeSpan), window, "teclc"));
                });
                Thread.Sleep(timeSpan * 1.35);
                window.Dispatcher.Invoke(act);
            });
        }
    }
}
