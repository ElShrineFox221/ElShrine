using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ElShrine.Old.Wpf
{
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public static class Methods
    {
        public static DispatcherTimer OpacityChange(UIElement target, TimeSpan layout, TimeSpan wait, TimeSpan layin, double targetOp, double originalOp, List<EventHandler?> opacityChangeHandlers)
        {
            DispatcherTimer dt = new() { Interval = layout };
            int count = 0;
            if (opacityChangeHandlers.Count >= count && opacityChangeHandlers[count] != null)
            {
                dt.Tick += opacityChangeHandlers[count];
                count++;
            }
            dt.Tick += delegate
            {
                if (opacityChangeHandlers.Count > count && opacityChangeHandlers[count] != null)
                {
                    dt.Tick -= opacityChangeHandlers[count - 1];
                    dt.Tick += opacityChangeHandlers[count];
                }
                else if (opacityChangeHandlers[count - 1] != null)
                {
                    dt.Tick -= opacityChangeHandlers[count - 1];
                }
                switch (count)
                {
                    case 1: dt.Interval = wait; count++; break;
                    case 2: dt.Interval = layin; count++; break;
                    default: dt.Stop(); break;
                }
            };
            target.Dispatcher.Invoke(() =>
            {
                dt.Start();
                target.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation() { EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut, }, To = targetOp, Duration = layout }, HandoffBehavior.Compose);
                target.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation() { EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut, }, To = originalOp, Duration = layin, BeginTime = layout + wait }, HandoffBehavior.Compose);
            });
            return dt;
        }
        public static DispatcherTimer OpacityChange(UIElement target, TimeSpan layout, TimeSpan wait, TimeSpan layin, double targetOp, double originalOp)
            => OpacityChange(target, layout, wait, layin, targetOp, originalOp, [null, null, null]);

        public static DispatcherTimer SimpleAnimation<A, T>(A animation, Action? action, T owner, DependencyProperty dp) where A : AnimationTimeline where T : DispatcherObject, IAnimatable
        {
            DispatcherTimer dt = new() { Interval = animation.Duration.TimeSpan, };
            owner.Dispatcher.Invoke(() => {
                owner.BeginAnimation(dp, animation, HandoffBehavior.Compose);
                dt.Start();
            });
            dt.Tick += delegate { if (action != null) owner.Dispatcher.Invoke(action); dt.Stop(); };
            return dt;
        }
        public static DispatcherTimer SimpleDoubleAnimation<T>(Duration? duration, double? to, double? from, Action? action, T owner, DependencyProperty dp, TimeSpan? beginTime = null) where T : DispatcherObject, IAnimatable
        {
            return SimpleAnimation(new DoubleAnimation() { EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut, }, To = to, From = from, BeginTime = beginTime ?? TimeSpan.FromMilliseconds(0), Duration = duration ?? TimeSpan.FromMilliseconds(Option.GetInstance().LayTimeSpanMS), }, action, owner, dp);
        }
        public static DispatcherTimer SimpleColorAnimation<T>(Duration? duration, Color? to, Color? from, Action? action, T owner, DependencyProperty dp, TimeSpan? beginTime) where T : DispatcherObject, IAnimatable
        {
            return SimpleAnimation(new ColorAnimation() { From = from, To = to, Duration = duration ?? TimeSpan.FromMilliseconds(Option.GetInstance().LayTimeSpanMS), BeginTime = beginTime ?? TimeSpan.FromMilliseconds(0), EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut, }, }, action, owner, dp);
        }
    }
}
