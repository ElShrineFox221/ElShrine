using ElShrine.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ElShrine.Old.Wpf.Controls
{
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public partial class WindowEvent
    {
        #region Window
        private static double Od { get=> Math.Max(Option.GetInstance().Option_Opacity, Option.GetInstance().Option_Opacity_Min); }
        private void ElShrineWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = (Window)sender;
            if (window.IsTabStop) return;
            window.IsTabStop = true;
            Window moveWindow = new()
            {
                Width = window.ActualWidth,
                Height = window.ActualHeight,
                WindowStartupLocation = WindowStartupLocation.Manual,
                WindowStyle = WindowStyle.None,
                Style = (Style)Application.Current.FindResource("ElShrineWindow_Style"),
                DataContext = window.DataContext,
                ShowInTaskbar = false,
            };
            RefreshMoveWindow();

            Point p;
            Point correctP = new(0, 0);

            window.Opacity = Od;
            moveWindow.Opacity = 0;

            bool confrimed = false;
            window.MouseDown += delegate (object sender, MouseButtonEventArgs e)
            {
                if (e.Source is not Window) return;
                p = Mouse.GetPosition(window);
                moveWindow.Focus();
                moveWindow.CaptureMouse();
                RefreshMoveWindow();
                confrimed = false;
                moveWindow.Visibility = Visibility.Visible;

                window.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation() { EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut, }, Duration = new(TimeSpan.FromMilliseconds(Option.GetInstance().LayTimeSpanMS)), To = Od / 2 }, HandoffBehavior.Compose);
                moveWindow.Opacity = Od;
            };
            moveWindow.PreviewMouseMove += delegate (object sender, MouseEventArgs e)
            {

                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point p1 = window.PointToScreen(Mouse.GetPosition(window));
                    p1.Offset(-p.X, -p.Y);
                    p1.Offset(correctP.X, correctP.Y);
                    moveWindow.Left = p1.X; moveWindow.Top = p1.Y;
                }
            };

            moveWindow.PreviewMouseUp += delegate (object sender, MouseButtonEventArgs e)
            {
                DragingConfrim();
            };
            moveWindow.MouseLeave += delegate
            {
                DragingConfrim();
            };
            bool DragingConfrimed()
            {
                if (window.Left == moveWindow.Left && window.Top == moveWindow.Top && confrimed) return true;
                confrimed = true;
                return false;
            }
            void DragingConfrim()
            {
                if (DragingConfrimed()) return;
                window.Left = moveWindow.Left;
                window.Top = moveWindow.Top;
                moveWindow.ReleaseMouseCapture();
                window.Focus();
                window.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation() { EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut, }, Duration = new(TimeSpan.FromMilliseconds(Option.GetInstance().LayTimeSpanMS)), To = Od }, HandoffBehavior.Compose);
                moveWindow.Opacity = 0;
            }
            void RefreshMoveWindow()
            {
                moveWindow.Width = window.ActualWidth;
                moveWindow.Height = window.ActualHeight;
            }
        }
        #endregion
        public static void ElShrineWindow_HeaderOpenButton_Click(object sender, bool opacityChange)
        {
            Window? window = Methods.FindParent<Window>((DependencyObject)sender);
            if (window == null) return;
            Border? HeaderPanel = (Border?)Methods.FindChild(window, "HeaderPanel");
            Border? MainPanel = (Border?)Methods.FindChild(window, "MainPanel");
            if (HeaderPanel == null || MainPanel == null) return;
            HeaderPanel.BeginAnimation(FrameworkElement.WidthProperty,
                new DoubleAnimation()
                {
                    EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut, },
                    From = (HeaderPanel.Opacity > 0.1) ? Option.GetInstance().WindowHeaderWidth : 0,
                    To = (HeaderPanel.Opacity < 0.1) ? Option.GetInstance().WindowHeaderWidth : 0,
                    Duration = 2 * TimeSpan.FromMilliseconds(Option.GetInstance().LayTimeSpanMS),
                });
            HeaderPanel.BeginAnimation(UIElement.OpacityProperty,
                new DoubleAnimation()
                {
                    EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut, },
                    To = (HeaderPanel.Opacity < 0.1) ? Option.GetInstance().Option_Opacity : 0,
                    Duration = 2 * TimeSpan.FromMilliseconds(Option.GetInstance().LayTimeSpanMS),
                });
            if (opacityChange) MainPanel.BeginAnimation(UIElement.OpacityProperty,
                new DoubleAnimation()
                {
                    EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut, },
                    To = (HeaderPanel.Opacity > 0.1) ? Option.GetInstance().Option_Opacity : 0.5,
                    Duration = 2 * TimeSpan.FromMilliseconds(Option.GetInstance().LayTimeSpanMS),
                });
        }
        public static void AddHeaders(Window window, List<string> names, List<object> styles, List<double> fontSizes, List<RoutedEventHandler?>? delegates)
        {
            if (window.Style != (Style)Application.Current.FindResource("ElShrineWindow_HeaderStyle")) return;
            ItemsControl? HeaderPanel = (ItemsControl?)Methods.FindChild(window, "HeaderItemsPanel");
            ObservableCollection<Button> Buttons = [];
            if (HeaderPanel == null) return;
            object? style = styles[0];
            double fontSize = fontSizes[0];
            RoutedEventHandler? @delegate = delegates?[0];
            foreach (string name in names)
            {
                if (names.IndexOf(name) <= styles.Count - 1) style = styles[names.IndexOf(name)];
                if (names.IndexOf(name) <= fontSizes.Count - 1) fontSize = fontSizes[names.IndexOf(name)];
                if (delegates != null) if (names.IndexOf(name) <= delegates.Count - 1) @delegate = delegates[names.IndexOf(name)];
                Button HeaderButton = new()
                {
                    Content = name,
                    Style = (Style)style,
                    FontSize = fontSize,
                    FontWeight = FontWeights.Bold,
                };
                if (@delegate != null) HeaderButton.Click += @delegate;
                Buttons.Add(HeaderButton);
            }
            HeaderPanel.ItemsSource = Buttons;
        }
        public static void AddHeaders(Window window, List<string> names, List<RoutedEventHandler?>? delegates)
        {
            AddHeaders(window, names, [Application.Current.FindResource("ElShrineButton_UnderLineStyle")], [16], delegates);
        }
    }
}
