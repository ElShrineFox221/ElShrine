using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ElShrine.Wpf
{
    public static class Extensions
    {
        #region Property Helpers

        /// <summary>
        /// 去除属性名称中的 "Property" 后缀
        /// </summary>
        public static string ToPropRegName(this string name) => name.Replace("Property", string.Empty);
        #endregion

        #region Visual Rendering (Snapshot)

        public static RenderTargetBitmap CreateVisualSnapshot(this Visual visual, double minWidth = 1, double minHeight = 1)
        {
            var bounds = VisualTreeHelper.GetDescendantBounds(visual);
            minWidth = Math.Max(minWidth, 1);
            minHeight = Math.Max(minHeight, 1);

            // 使用 Ceiling 确保尺寸足够，避免舍入导致的黑边
            int width = (int)Math.Ceiling(Math.Max(bounds.Width, minWidth));
            int height = (int)Math.Ceiling(Math.Max(bounds.Height, minHeight));

            var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                var vb = new VisualBrush(visual);
                dc.DrawRectangle(vb, null, new Rect(new Point(0, 0), bounds.Size));
            }
            rtb.Render(dv);
            return rtb;
        }
        #endregion

        #region Visual Tree - Find Parent (Upwards)

        /// <summary>
        /// 向上查找符合条件的父元素
        /// </summary>
        public static FrameworkElement? FindParent(this DependencyObject child, Predicate<DependencyObject> predicate, bool selfIncluded = true)
        {
            var current = selfIncluded ? child : VisualTreeHelper.GetParent(child);
            while (current is not null)
            {
                if (predicate(current))
                {
                    return current as FrameworkElement;
                }
                current = (current is FrameworkElement fe ? fe.Parent : null) ?? VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        /// <summary>
        /// 向上查找指定类型的父元素
        /// </summary>
        public static T? FindParent<T>(this DependencyObject child, bool selfIncluded = true) where T : DependencyObject
        {
            var result = child.FindParent(d => d is T, selfIncluded);
            return result as T;
        }

        /// <summary>
        /// 向上查找指定名称的父元素
        /// </summary>
        public static FrameworkElement? FindParent(this FrameworkElement child, string name, bool selfIncluded = true)
        {
            return child.FindParent(d => d is FrameworkElement fe && fe.Name == name, selfIncluded);
        }
        #endregion

        #region Visual Tree - Find Child (Downwards)
        /// <summary>
        /// 递归向下查找符合条件的子元素
        /// </summary>
        public static FrameworkElement? FindChild(this DependencyObject parent, Predicate<DependencyObject> predicate)
        {
            if (parent == null) return null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (predicate(child)) return child as FrameworkElement;

                var found = child.FindChild(predicate);
                if (found != null) return found;
            }

            return null;
        }

        /// <summary>
        /// 递归查找指定类型的子元素
        /// </summary>
        public static T? FindChild<T>(this Visual parent) where T : Visual
        {
            var result = parent.FindChild(d => d is T);
            return result as T;
        }

        /// <summary>
        /// 递归查找指定名称的子元素
        /// </summary>
        public static FrameworkElement? FindChild(this FrameworkElement parent, string childName)
        {
            return parent.FindChild(d => d is FrameworkElement fe && fe.Name == childName);
        }
        #endregion

        #region Visual Tree - Misc (Relationship & Position)

        /// <summary>
        /// 获取根节点
        /// </summary>
        public static DependencyObject GetRootDependencyObject(this Visual element)
        {
            DependencyObject current = element;
            while (true)
            {
                var parent = VisualTreeHelper.GetParent(current);
                if (parent == null) return current;
                current = parent;
            }
        }

        /// <summary>
        /// 判断是否是某个元素的子级
        /// </summary>
        public static bool IsChildOf(this DependencyObject child, DependencyObject parent)
        {
            var current = child;
            while (current != null)
            {
                if (current == parent) return true;
                current = VisualTreeHelper.GetParent(current);
            }
            return false;
        }

        /// <summary>
        /// 获取相对于另一个 Visual 的坐标位置
        /// </summary>
        public static Point GetPositionRelativeTo(this Visual target, Visual source)
            => target.TransformToVisual(source).Transform(new Point(0, 0));
        #endregion
    }
}
