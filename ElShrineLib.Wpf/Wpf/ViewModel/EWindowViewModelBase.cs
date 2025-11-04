using ElShrine.Old.Wpf.Controls;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using static ElShrine.Old.Wpf.Option;
using static ElShrine.Wpf.Methods;

namespace ElShrine.Wpf.ViewModel
{
    public partial class EWindowViewModelBase : ViewModelBase<Window>
    {
        protected static WpfOption WpfOption => WpfOption.GetInstance();
        protected static readonly List<Window> ownerWindows = [];
        public static T SetWindowViewModel<T>(Window ownerWindow) where T : EWindowViewModelBase
        {
            #region ViewModel Instance Building
            T viewmodel = (T?)Activator.CreateInstance(
                typeof(T), 
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, 
                null, 
                [ownerWindow], 
                null) 
                ?? throw new("Cannot build a instance of the viewmodel.");
            ownerWindow.DataContext = viewmodel;
            ownerWindows.Add(ownerWindow);
            #endregion
            //viewmodel

            #region System Command Bindings
            var cb = ownerWindow.CommandBindings;
            cb.Add(new(SystemCommands.CloseWindowCommand, (_, _) => { viewmodel.Window_Close.ExecuteAction?.Invoke(null); }));
            cb.Add(new(SystemCommands.MinimizeWindowCommand, (_, _) => { viewmodel.Window_FadeOut.ExecuteAction?.Invoke(0.5d); }));
            cb.Add(new(SystemCommands.MaximizeWindowCommand, (_, _) => { SystemCommands.MaximizeWindow(ownerWindow); }));
            cb.Add(new(SystemCommands.RestoreWindowCommand, (_, _) => { SystemCommands.RestoreWindow(ownerWindow); }));
            cb.Add(new(SystemCommands.ShowSystemMenuCommand, (sender, e) =>
            {
                if (e.OriginalSource is not FrameworkElement element) return;
                var position = ownerWindow.WindowState == WindowState.Maximized ? new Point(0, element.ActualHeight)
                    : new Point(ownerWindow.Left + ownerWindow.BorderThickness.Left, element.ActualHeight + ownerWindow.Top + ownerWindow.BorderThickness.Top);
                position = element.TransformToAncestor(ownerWindow).Transform(position);
                SystemCommands.ShowSystemMenu(ownerWindow, position);
            }));
            #endregion

            return viewmodel;
        }
        
        protected EWindowViewModelBase(Window ownerWindow) : base(ownerWindow) { this.ownerWindow = ownerWindow; }

        protected Window? ownerWindow;

        private static Duration FadeDuration => TimeSpan.FromMilliseconds(WpfOption.FadeInOutms);
        protected virtual void WindowFadeOut(Action? action, double factor)
        {
            if (ownerWindow == null) return;
            SimpleDoubleAnimation(FadeDuration.Factor(factor), 0, null, () => {
                SystemCommands.MinimizeWindow(ownerWindow);
                ownerWindow.SetValue(UIElement.OpacityProperty, GetInstance().Option_Opacity);
                action?.Invoke();
            }, ownerWindow, UIElement.OpacityProperty);
        }
        protected virtual void WindowFadeIn(Action? action, double factor)
        {
            if (ownerWindow == null) return;
            ownerWindow.SetValue(UIElement.OpacityProperty, 0);
            SimpleDoubleAnimation(FadeDuration.Factor(0), 0, null, null, ownerWindow, UIElement.OpacityProperty);
            SimpleDoubleAnimation(FadeDuration.Factor(factor), GetInstance().Option_Opacity, null, () => {
                ownerWindow.SetValue(UIElement.OpacityProperty, GetInstance().Option_Opacity);
                action?.Invoke(); 
            }, ownerWindow, UIElement.OpacityProperty);
        }

        public VMCommand Window_FadeOut => new(o =>
        {
            if (o is not double i) i = 1;
            //CommandParse
            if (ownerWindow == null) return;
            WindowFadeOut(null, i);
        });
        public VMCommand Window_FadeIn => new(o =>
        {
            if (o is not double i) i = 1;
            //CommandParse
            if (ownerWindow == null) return;
            WindowFadeIn(null, i);
        });
        public VMCommand Window_Close => new(o =>
        {
            if (o is not double i) i = 1;
            //CommandParse
            if (ownerWindow == null) return;
            WindowFadeOut(() => {
                SystemCommands.CloseWindow(ownerWindow);
                ownerWindows.Remove(ownerWindow);
                if (ownerWindows.Count == 0) Environment.Exit(0);
            }, i);
        });
        public VMCommand Window_CloseAll => new(o =>
        {
            //CommandParse
            foreach (Window window in ownerWindows)
            {
                if (o is not double i) i = 1;
                WindowFadeOut(() =>
                {
                    SystemCommands.CloseWindow(window);
                    ownerWindows.Remove(window);
                    Environment.Exit(0);
                }, i);
            }
        });

        protected bool headersDropped = true;
        protected bool HeadersDropped
        {
            get { return headersDropped; }
            set
            {
                if (ownerWindow != null && headersDropped != value) WindowEvent.ElShrineWindow_HeaderOpenButton_Click(ownerWindow, false);
                headersDropped = value;
            }
        }
        public VMCommand Window_DropHeader => new(o => {
            if (o == null || (int)o == 0 ^ HeadersDropped) HeadersDropped = !HeadersDropped;
        });

        private double window_Width = 800;
        public double Window_Width
        {
            get => window_Width;
            set
            {
                window_Width = value;
                Window_ScaleHeightToWidth = Window_Width / Window_Height;
                NoticePropertyChanged(nameof(window_Width), nameof(Window_ScaleHeightToWidth));
            }
        }
        private double window_Height = 600;
        public double Window_Height
        {
            get => window_Height;
            set
            {
                window_Height = value;
                Window_ScaleHeightToWidth = Window_Width / Window_Height;
                NoticePropertyChanged(nameof(window_Height), nameof(Window_ScaleHeightToWidth));
            }
        }
        private double window_ScaleHeightToWidth = 4d / 3d;
        public double Window_ScaleHeightToWidth
        {
            get => window_ScaleHeightToWidth;
            set
            {
                window_ScaleHeightToWidth = value;
                NoticePropertyChanged(this, nameof(Window_ScaleHeightToWidth));
            }
        }
    }
}
