using ElShrine.Old.Wpf;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using Color = System.Windows.Media.Color;

namespace ElShrine.Wpf
{
    public static class Methods
    {
        #region Converter: string(hex) - byte
        public static string ByteArrayHexConverte(params byte[] bytes)
        {
            string result = string.Empty;
            foreach (byte b in bytes)
            {
                result += Convert.ToString(b, 16).PadLeft(2, '0').ToUpper();
            }
            return result;
        }
        public static byte[] ByteArrayHexConverte(string hexStr)
        {
            List<byte> bytes = [];
            char[] chars = hexStr.ToCharArray();
            int i = 2;
            while (i < chars.Length)
            {
                if (byte.TryParse(new string(chars[i - 2], chars[i - 1]), out byte ob)) bytes.Add(ob);
                i += 2;
            }
            return [.. bytes];
        }
        #endregion

        #region Factor
        public static Color ColorFactor(Color color, double rate, bool changeAlpha = false)
            => ColorFactor(color, rate, changeAlpha ? -1 : color.A);
        public static Color ColorFactor(Color color, double offsetRate, int alpha)
        {
            Color returnColor = Color.FromArgb((byte)(color.A * (1 - offsetRate)), (byte)(color.R * (1 - offsetRate)), (byte)(color.G * (1 - offsetRate)), (byte)(color.B * (1 - offsetRate)));
            if (alpha != -1) returnColor.A = (byte)alpha;
            return returnColor;
        }
        public static Color ColorFactor(Color color, Color targetColor, double offsetRate)
        {
            (byte A, byte R, byte G, byte B) = ColorArgbCheck(ColorAdd(ColorFact((color.A, color.R, color.G, color.B), 1 - offsetRate), ColorFact((targetColor.A, targetColor.R, targetColor.G, targetColor.B), offsetRate)));
            return Color.FromArgb(A, R, G, B);
        }

        public static (double A, double R, double G, double B) ColorFact((double A, double R, double G, double B) color, double factor)
        {
            double ad = color.A * factor;
            double rd = color.R * factor;
            double gd = color.G * factor;
            double bd = color.B * factor;
            return (ad, rd, gd, bd);
        }
        public static (double A, double R, double G, double B) ColorAdd((double A, double R, double G, double B) color1, (double A, double R, double G, double B) color2, bool isSub = false)
        {
            double ad = isSub ? color1.A - color2.A : color1.A + color2.A;
            double rd = isSub ? color1.R - color2.R : color1.R + color2.R;
            double gd = isSub ? color1.G - color2.G : color1.G + color2.G;
            double bd = isSub ? color1.B - color2.B : color1.B + color2.B;
            return (ad, rd, gd, bd);
        }
        #endregion

        #region DurationFactor
        public static Duration Factor(this Duration duration, double factor)
            => duration.TimeSpan * factor;
        #endregion

        //Using
        private static (byte A, byte R, byte G, byte B) ColorArgbCheck((double A, double R, double G, double B) color)
            => ColorArgbCheck((int)color.A, (int)color.R, (int)color.G, (int)color.B);
        private static (byte A, byte R, byte G, byte B) ColorArgbCheck(int a, int r, int g, int b)
        {
            a = a < 0 ? 0 : a > 255 ? 255 : a;
            r = r < 0 ? 0 : r > 255 ? 255 : r;
            g = g < 0 ? 0 : g > 255 ? 255 : g;
            b = b < 0 ? 0 : b > 255 ? 255 : b;
            return ((byte)a, (byte)r, (byte)g, (byte)b);
        }

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

        public static DispatcherTimer SimpleAnimation<A, T>(A animation, Action? action, T owner, DependencyProperty dp) where A : AnimationTimeline where T : DispatcherObject,IAnimatable
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
            return SimpleAnimation(new DoubleAnimation(){ EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut, }, To = to, From = from, BeginTime = beginTime ?? TimeSpan.FromMilliseconds(0), Duration = duration ?? TimeSpan.FromMilliseconds(Option.GetInstance().LayTimeSpanMS), }, action, owner, dp);
        }
        public static DispatcherTimer SimpleColorAnimation<T>(Duration? duration, Color? to, Color? from, Action? action, T owner, DependencyProperty dp,TimeSpan? beginTime) where T : DispatcherObject, IAnimatable
        {
            return SimpleAnimation(new ColorAnimation() { From = from, To = to, Duration = duration ?? TimeSpan.FromMilliseconds(Option.GetInstance().LayTimeSpanMS), BeginTime = beginTime ?? TimeSpan.FromMilliseconds(0), EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut, }, }, action, owner, dp);
        }
        public static RenderTargetBitmap CreateVisualSnapshot(this Visual visual, double minWidth = 1, double minHeight = 1)
        {
            var bounds = VisualTreeHelper.GetDescendantBounds(visual);
            minWidth = Math.Max(minWidth, 1); minHeight = Math.Max(minHeight, 1);
            var rtb = new RenderTargetBitmap((int)Math.Round(Math.Max(bounds.Width, minWidth)), (int)Math.Round(Math.Max(bounds.Height, minHeight)), 96, 96, PixelFormats.Pbgra32);
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                var vb = new VisualBrush(visual);
                dc.DrawRectangle(vb, null, bounds);
            }
            rtb.Render(dv);
            return rtb;
        }
        public static FrameworkElement? FindChild(this FrameworkElement Parent, string ChildName, bool deepSearch = false)
        {
            return GetChild(Parent, deepSearch);
            FrameworkElement? GetChild(FrameworkElement Parent, bool hasSearchedPeripherally)
            {
                int count = VisualTreeHelper.GetChildrenCount(Parent);
                if (!hasSearchedPeripherally)
                {
                    count = CheckParent(Parent);
                    switch (count)
                    {
                        case 0: return null;
                        case -1: return Parent;
                        default:
                            FrameworkElement? result = SuperficialSearchChild(Parent, count);
                            if (result != null) return result;
                            break;
                    }
                }
                FrameworkElement? Result = DeepSearchChild(Parent, count);
                return Result;
            }
            int CheckParent(FrameworkElement Parent)
            {
                if (Parent == null) return 0;
                if (Parent.Name == ChildName) return -1;
                else return VisualTreeHelper.GetChildrenCount(Parent);
            }
            FrameworkElement? SuperficialSearchChild(FrameworkElement Parent, int childCount)
            {
                for (int i = 0; childCount > i; i++)
                {
                    object childObject = VisualTreeHelper.GetChild(Parent, i);
                    if (childObject == null || childObject is not FrameworkElement) continue;
                    FrameworkElement child = (FrameworkElement)childObject;
                    if (child.Name == ChildName) return child;
                }
                return null;
            }
            FrameworkElement? DeepSearchChild(FrameworkElement Parent, int childCount)
            {
                for (int i = 0; childCount > i; i++)
                {
                    object childObject = VisualTreeHelper.GetChild(Parent, i);
                    if (childObject == null || childObject is not FrameworkElement) return null;
                    FrameworkElement? child = (FrameworkElement)childObject;
                    FrameworkElement? childChild = GetChild(child, false);
                    if (childChild != null) return childChild;
                }
                return null;
            }
        }
        public static T? FindChild<T>(this DependencyObject parent) where T : DependencyObject
        {
            T? result = null;
            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T r) result = r;
                else result = FindChild<T>(child);
                if (result is not null) break;
            }
            return result;
        }
        public static T? FindParent<T>(this DependencyObject child, bool selfIncluded = true) where T : DependencyObject
        {
            if (selfIncluded && child is T ct) return ct;
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            if (parentObject is T parent) return parent;
            else return FindParent<T>(parentObject, true);
        }
        public static FrameworkElement? FindParent(this FrameworkElement child, string name, bool ignoreCase = true)
        {
            if (equal(child.Name, name, ignoreCase)) return child;
            if (VisualTreeHelper.GetParent(child) is not FrameworkElement parentObject) return null;
            if (equal(parentObject.Name, name, ignoreCase)) return parentObject;
            else return FindParent(parentObject, name, ignoreCase);
            static bool equal(string name0, string name1, bool ignoreCase)
                => ignoreCase ? name0.EqualIgnoreCase(name0) : name1 == name0;
        }
        public static bool IsChildOf(this DependencyObject child, DependencyObject parent)
        {
            var currentParent = child;
            while (currentParent is not null && currentParent != parent) currentParent = VisualTreeHelper.GetParent(currentParent);
            return currentParent is not null;
        }
        public static Point GetPositionRelativeTo(this Visual target, Visual source)
        {
            Point screenPos = target.PointToScreen(default);
            return source.PointFromScreen(screenPos);
        }


        public static FrameworkElement? FindVisualParent(this FrameworkElement element, Type? parentType = null, string? name = null, bool selfInconcluded = true)
        {
            parentType ??= typeof(FrameworkElement);
            DependencyObject? resultObj = element;
            if (!selfInconcluded) resultObj = VisualTreeHelper.GetParent(element);
            while (resultObj is not null && !predict(resultObj)) resultObj = VisualTreeHelper.GetParent(resultObj);
            return resultObj as FrameworkElement;
            bool predict(DependencyObject dobj)
                => dobj.GetType().IsSubclassOf(parentType) && (name is null || (dobj is FrameworkElement ele && ele.Name == name));
        }
        public static FrameworkElement? FindVisualParent(this FrameworkElement element, Predicate<DependencyObject> predicate, bool selfInconcluded = true)
        {
            DependencyObject? resultObj = element;
            if (!selfInconcluded) resultObj = VisualTreeHelper.GetParent(element);
            while (resultObj is not null && !predicate.Invoke(resultObj)) resultObj = VisualTreeHelper.GetParent(resultObj);
            return resultObj as FrameworkElement;
        }
        public static T? FindVisualParent<T>(this FrameworkElement element, string? name = null, bool selfInconcluded = true) where T : FrameworkElement
            => (T?)element.FindVisualParent(typeof(T), name, selfInconcluded);
        public static FrameworkElement? FindVisualChildRecursive(this DependencyObject parent, Predicate<DependencyObject> predicate)
        {
            if (parent == null) return null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (predicate(child)) return child as FrameworkElement;
                var found = child.FindVisualChildRecursive(predicate);
                if (found != null) return found; 
            }
            return null; 
        }
        public static DependencyObject GetRootDependencyObject(this DependencyObject element)
        {
            DependencyObject? parent = element;
            while (parent != null)
            {
                element = parent;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return element;
        }

    }
}
