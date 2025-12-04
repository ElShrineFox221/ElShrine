using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ElShrine.Wpf.Controls.Extensions
{
    /// <summary>
    /// 实现了根据绑定源的 IsEnabled 属性，对目标元素执行 Opacity 淡入淡出动画的附加属性组。
    /// 动画目标：应用了此附加属性的 UIElement/Brush。
    /// 状态源：EnabledSource 附加属性指定的 UIElement。
    /// </summary>
    public static class EnabledTrans
    {
        #region DPs
        /// <summary>
        /// DisabledOpacity 附加属性：定义目标对象在源控件禁用时的透明度（0.0 到 1.0）。
        /// </summary>
        public static readonly DependencyProperty DisabledOpacityProperty = DependencyProperty.RegisterAttached("DisabledOpacity", typeof(double), typeof(EnabledTrans), new PropertyMetadata(0.3d));
        public static double GetDisabledOpacity(DependencyObject obj) => (double)obj.GetValue(DisabledOpacityProperty);
        public static void SetDisabledOpacity(DependencyObject obj, double value) => obj.SetValue(DisabledOpacityProperty, value);
        /// <summary>
        /// EnabledSource 附加属性：指定一个 UIElement，其 IsEnabled 状态将驱动透明度动画。
        /// </summary>
        public static readonly DependencyProperty EnabledSourceProperty = DependencyProperty.RegisterAttached("EnabledSource", typeof(UIElement), typeof(EnabledTrans), new PropertyMetadata(null, OnEnabledSourceChanged));
        public static UIElement GetEnabledSource(DependencyObject obj) => (UIElement)obj.GetValue(EnabledSourceProperty);
        public static void SetEnabledSource(DependencyObject obj, UIElement value) => obj.SetValue(EnabledSourceProperty, value);
        #endregion

        private static readonly ConditionalWeakTable<UIElement, List<WeakReference<DependencyObject>>> registeredDPOs = [];
        private static void OnEnabledSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is UIElement oldSource && registeredDPOs.TryGetValue(oldSource, out var list))
            {
                list.RemoveAll(wr => !wr.TryGetTarget(out var target) || target == d);
                if (list.Count == 0) ClearSource(oldSource);
                if (e.NewValue == null) DoTransition(d, true);
            }
            if (e.NewValue is UIElement newSource)
            {
                list = registeredDPOs.GetValue(newSource, (key) =>
                {
                    key.IsEnabledChanged += Source_IsEnabledChanged;
                    return [];
                });
                list.Add(new(d));
                DoTransition(d, newSource.IsEnabled);
            }
        }
        private static void Source_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not UIElement sourceElement) return;
            if (registeredDPOs.TryGetValue(sourceElement, out var list))
            {
                bool isEnabled = (bool)e.NewValue;
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var weakRef = list[i];
                    if (weakRef.TryGetTarget(out var dependencyObject)) DoTransition(dependencyObject, isEnabled);
                    else list.RemoveAt(i);
                }
                if (list.Count == 0) ClearSource(sourceElement);
            }
        }
        private static void ClearSource(UIElement sourceElement)
        {
            registeredDPOs.Remove(sourceElement);
            sourceElement.IsEnabledChanged -= Source_IsEnabledChanged;
        }

        #region 动画逻辑
        public const double DefaultDurationSeconds = 0.2;
        /// <summary>
        /// 执行透明度动画。
        /// </summary>
        /// <param name="targetObject">应用了 EnabledSource 的对象（要动画 Opacity 的对象）。</param>
        /// <param name="toEnabled">动画目标状态 (true 为启用/不透明, false 为禁用/透明)。</param>
        private static void DoTransition(DependencyObject targetObject, bool toEnabled)
        {
            //get target dp
            DependencyProperty? targetDP;
            TransHelper.GetThemeControlParent(targetObject, out _, out var themeControl);
            targetDP = (targetObject is UIElement ? UIElement.OpacityProperty : null) ?? (targetObject is Brush ? Brush.OpacityProperty : null);
            if (targetDP is null) return;
            //trans params
            var toValue = toEnabled ? 1 : GetDisabledOpacity(targetObject);
            var anim = new DoubleAnimation()
            {
                To = toValue,
                EasingFunction = themeControl?.AnimaEaseFunc,
                Duration = TimeSpan.FromSeconds((toEnabled ? themeControl?.AnimaDurationIn : themeControl?.AnimaDurationOut) ?? DefaultDurationSeconds),
            };
            if (targetObject is UIElement uIElement) uIElement.BeginAnimation(targetDP, anim);
            else if(targetObject is Animatable animatable)
            {
                if (!animatable.IsFrozen) animatable.BeginAnimation(targetDP, anim);
                else animatable.SetValue(targetDP, toValue);
            }
        }
        #endregion
    }
}