using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;

namespace ElShrine.Wpf.Controls
{
    public record AnimationsInfo(Duration BeginTime, Duration BaseDuration, FrameworkElement Target)
    {
        public readonly List<(AnimationTimeline a, DependencyProperty dP)> Animations = [];
        public Action? CompleteAction;
        protected virtual void Confrim()
        {
            AnimationTimeline? maxDuraAnim = null; Duration maxDura = new(TimeSpan.Zero);
            for (var i = 0; i < Animations.Count; i++)
            {
                var (a, dP) = Animations[i];
                a.Completed += (_, _) =>
                {
                    var value = Target.GetValue(dP);
                    Target.BeginAnimation(dP, null);
                    Target.SetValue(dP, value);
                };
                if (a.BeginTime < BeginTime.TimeSpan) a.BeginTime = BeginTime.TimeSpan;
                var dura = a.Duration + a.BeginTime;
                if (dura.HasValue && dura > maxDura)
                {
                    maxDuraAnim = a;
                    maxDura = dura.Value;
                }
            }
            if (maxDuraAnim is not null) maxDuraAnim.Completed += (_, _) => CompleteAction?.Invoke();
        }
        protected virtual void Play(Storyboard? carrier = null)
        {
            carrier ??= new();
            AppendToStoryboard(carrier);
            carrier.Begin();
        }
        protected void AppendToStoryboard(Storyboard storyboard)
        {
            for (var i = 0; i < Animations.Count; i++)
            {
                var (a, dP) = Animations[i];
                if (!storyboard.Children.Contains(a)) storyboard.Children.Add(a);
                Storyboard.SetTarget(a, Target);
                Storyboard.SetTargetProperty(a, new(dP));
                //ConsoleManager.ListContentInfo($"Animation started: {Target.Name}, {dP.Name}, {a.GetType()}, ms={DateTime.Now.Microsecond}");
            }
        }
        protected static void PlayAnimations(AnimationsInfo[] animations)
        {
            var storyboard = new Storyboard();
            foreach (var anim in animations) anim.AppendToStoryboard(storyboard);
            storyboard.Begin();
        }
        public static void PlayAnimations(AnimationsInfo[] animations, AnimationInfoTriggerMode triggerMode, Action? finishedCallBack = null)
        {
            //Confrim animations
            foreach(var anim in animations) anim.Confrim();
            //Adjust trigger
            if (animations.Length == 0) triggerMode = AnimationInfoTriggerMode.None;
            if (triggerMode == AnimationInfoTriggerMode.InReversedOrder) 
            {
                animations.Reverse();
                triggerMode = AnimationInfoTriggerMode.InOrder;
            }
            //Associate and play
            switch (triggerMode)
            {
                case AnimationInfoTriggerMode.None:
                    finishedCallBack?.Invoke();
                    break;
                case AnimationInfoTriggerMode.All:
                    AnimationsInfo? maxDuraAnim = null; Duration maxDura = new(TimeSpan.Zero);
                    for (var i = 0; i < animations.Length; i++)
                    {
                        var anim1 = animations[i];
                        var dura = anim1.BaseDuration + anim1.BeginTime;
                        if (dura > maxDura)
                        {
                            maxDuraAnim = anim1;
                            maxDura = dura; 
                        }
                    }
                    if (maxDuraAnim is not null) maxDuraAnim.CompleteAction = finishedCallBack;
                    PlayAnimations(animations);
                    break;
                case AnimationInfoTriggerMode.InOrder:
                    var anim = animations[0];
                    for (var i = 1; (i < animations.Length); i++)
                    {
                        anim.CompleteAction = () => animations[i].Play();
                        anim = animations[i];
                    }
                    anim.CompleteAction = finishedCallBack;
                    animations[0].Play();
                    break;
            }
        }
    }
    public record AnimationsInfo<T, D> : AnimationsInfo where T : FrameworkElement
    {
        public AnimationsInfo(Duration beginTime, Duration baseDuration, T target, D appendedData) : base(beginTime, baseDuration, target) => AppendedData = appendedData;
        public new T Target => (T)base.Target;
        public D AppendedData;
    }
    public enum AnimationInfoTriggerMode
    {
        None = 0, All, InOrder, InReversedOrder
    }
}
