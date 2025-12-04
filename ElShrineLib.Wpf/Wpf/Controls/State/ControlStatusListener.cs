using ElShrine.Common;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ElShrine.Wpf.Controls.State
{
    /// <summary>
    /// 静态类，用于监听 WPF 控件的交互事件，实时评估并管理控件的组合状态和最高优先级状态。
    /// 该管理器使用位运算高效地确定状态优先级。
    /// </summary>
    public static class ControlStatusListener
    {
        #region event handlers
        private static void UIElement_StatusChanged(object sender, RoutedEventArgs e) => (sender as UIElement)?.UpdateStatus();
        private static void UIElement_StatusChangedWithMouseButton(object sender, RoutedEventArgs e)
        {
            if (sender is UIElement element && e is MouseButtonEventArgs mbe)
            {
                if (mbe.LeftButton == MouseButtonState.Pressed && Mouse.Captured is null)
                {
                    if (element is FrameworkElement fe && fe.FindVisualParent(p => p is ButtonBase) is null) element.CaptureMouse();
                }
                else if (Mouse.Captured == element) element.ReleaseMouseCapture();
                element.UpdateStatus();
            }
        }
        private static void UIElement_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) => (sender as UIElement)?.UpdateStatus();
        private static void Control_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is UIElement element) Unregister(element);
        }
        #endregion

        private static void UpdateStatus(this UIElement element, ControlStatus? targetStatus = null)
        {
            var initial = !statusByEle.TryGetValue(element, out var oldStatus);
            if (initial) statusByEle[element] = oldStatus = ControlStatus.Normal; 
            var newStatus = targetStatus ?? EvaluateStatus(element);
            if (newStatus != oldStatus || initial)
            {
                statusByEle[element] = newStatus;
                var e = new ValueChangedEventArgs<ControlStatus>(oldStatus, newStatus);
                StatusChanged?.Invoke(element, e);
                //
                var oldPrioStatus = EvaluatePrioStatus(oldStatus);
                var newPrioStatus = EvaluatePrioStatus(newStatus);
                if (oldPrioStatus != newPrioStatus) PrioStatusChanged?.Invoke(element, e);
            }
        }
        /// <summary>
        /// 当控件的组合状态（<see cref="ControlStatus"/> 的位组合）发生变化时触发。
        /// <para>此事件返回的是所有当前活动的位状态组合。</para>
        /// </summary>
        public static event ValueChangedHandler<ControlStatus>? StatusChanged;
        /// <summary>
        /// 当控件的最高优先级状态发生变化时触发。
        /// <para>此事件返回的是根据位值确定的、具有最高优先级的排他性状态。</para>
        /// </summary>
        public static event ValueChangedHandler<ControlStatus>? PrioStatusChanged;

        private static readonly Dictionary<UIElement, ControlStatus> statusByEle = [];
        /// <summary>
        /// 注册一个 UIElement 以监听其状态变化。
        /// 注册将附加所有必要的事件处理程序，并初始化控件的当前状态。
        /// </summary>
        /// <param name="element">需要监听的 UIElement 控件。</param>
        /// <returns>如果元素是第一次被注册，返回 true；如果元素已被注册，返回 false。</returns>
        public static bool Register(UIElement element)
        {
            var notGot = !statusByEle.ContainsKey(element);
            if (notGot)
            {
                #region registration
                element.MouseEnter += UIElement_StatusChanged;
                element.MouseLeave += UIElement_StatusChanged;
                element.GotMouseCapture += UIElement_StatusChanged;
                element.LostMouseCapture += UIElement_StatusChanged;
                element.GotFocus += UIElement_StatusChanged;
                element.LostFocus += UIElement_StatusChanged;
                element.IsEnabledChanged += UIElement_IsEnabledChanged;

                element.MouseDown += UIElement_StatusChangedWithMouseButton;
                element.AddHandler(UIElement.MouseLeftButtonUpEvent, (RoutedEventHandler)UIElement_StatusChangedWithMouseButton, true);
                if (element is Control control)
                {
                    control.Unloaded += Control_Unloaded;
                    if(control is ToggleButton toggleBtn)
                    {
                        toggleBtn.Checked += UIElement_StatusChanged;
                        toggleBtn.Unchecked += UIElement_StatusChanged;
                    }
                }
                #endregion

                UpdateStatus(element);
            }
            return notGot;
        }
        /// <summary>
        /// 从状态监听器中注销一个 UIElement。
        /// 注销将移除所有附加的事件处理程序并清理内部缓存，以防止内存泄漏。
        /// </summary>
        /// <param name="element">需要注销的 UIElement 控件。</param>
        /// <returns>如果成功注销（元素先前已注册），返回 true；否则返回 false。</returns>
        public static bool Unregister(UIElement element)
        {
            var got = statusByEle.ContainsKey(element);
            if (got)
            {
                #region unregistration
                element.MouseEnter -= UIElement_StatusChanged;
                element.MouseLeave -= UIElement_StatusChanged;
                element.GotMouseCapture -= UIElement_StatusChanged;
                element.LostMouseCapture -= UIElement_StatusChanged;
                element.IsEnabledChanged -= UIElement_IsEnabledChanged;
                element.GotFocus -= UIElement_StatusChanged;
                element.LostFocus -= UIElement_StatusChanged;

                element.MouseDown -= UIElement_StatusChangedWithMouseButton;
                element.RemoveHandler(UIElement.MouseLeftButtonUpEvent, (RoutedEventHandler)UIElement_StatusChangedWithMouseButton);
                if (element is Control control)
                {
                    control.Unloaded -= Control_Unloaded;
                    if (control is ToggleButton toggleBtn)
                    {
                        toggleBtn.Checked -= UIElement_StatusChanged;
                        toggleBtn.Unchecked -= UIElement_StatusChanged;
                    }
                }
                #endregion

                UpdateStatus(element, ControlStatus.Normal);
                statusByEle.Remove(element);
            }
            return got;
        }

        /// <summary>
        /// 评估并返回控件当前的所有活动状态的组合。
        /// </summary>
        /// <param name="element">要评估的 UIElement。</param>
        /// <returns>一个包含所有活动状态的 <see cref="ControlStatus"/> 枚举组合值。</returns>
        public static ControlStatus EvaluateStatus(this UIElement element)
        {
            var status = ControlStatus.Normal;
            if (!element.IsEnabled) status |= ControlStatus.Disabled;
            var mouseDown = element.IsMouseCaptured || (element is FrameworkElement fe && fe.FindVisualParent(p => Mouse.Captured == p) is not null);
            if (mouseDown && element.IsEnabled) status |= ControlStatus.MouseDown;
            if (element is ToggleButton toggle && toggle.IsChecked == true) status |= ControlStatus.Checked;
            if ((element.IsFocused || element.IsKeyboardFocused) && element.IsEnabled) status |= ControlStatus.Focused;
            if ((mouseDown || element.IsMouseOver) && element.IsEnabled) status |= ControlStatus.MouseIn;
            return status;
        }
        /// <summary>
        /// 评估并返回给定 UIElement 的当前最高优先级状态。
        /// </summary>
        /// <param name="element">要评估的 UIElement。</param>
        /// <returns>具有最高位值（即最高优先级）的单个 <see cref="ControlStatus"/> 值。</returns>
        public static ControlStatus EvaluatePrioStatus(this UIElement element) => EvaluatePrioStatus(EvaluateStatus(element));
        /// <summary>
        /// 从一个组合状态中解析出最高优先级状态。
        /// <para>此方法通过位运算（查找最高有效位 MSB）来确定优先级，并返回对应的单一 <see cref="ControlStatus"/> 值。</para>
        /// </summary>
        /// <param name="status">要解析的 <see cref="ControlStatus"/> 组合值。</param>
        /// <returns>具有最高设置位（即最高优先级）的单个 <see cref="ControlStatus"/> 值。如果组合状态为 <c>0</c>，则返回 <see cref="ControlStatus.Normal"/>。</returns>
        public static ControlStatus EvaluatePrioStatus(this ControlStatus status)
        {
            const int BITS = sizeof(uint) * 8;
            var msbIndex = BITS - 1 - BitOperations.LeadingZeroCount((uint)status);
            return msbIndex >=0 ? (ControlStatus)(1 << msbIndex) : ControlStatus.Normal;
        }
    }
}
