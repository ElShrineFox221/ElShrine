using ElShrine.Wpf.Controls.Extensions;
using ElShrine.Wpf.Converters;
using System.Windows;
using System.Windows.Media.Animation;

namespace ElShrine.Wpf.Controls.Transitions
{
    public class DoubleTransition : ITransition
    {
        public bool TryDoTransition(DependencyObject tar, DependencyProperty tarDpProp, object? tarValue, bool isIn)
        {
            try
            {
                TransHelper.GetThemeControlParent(tar, out _, out var themeControl);
                var num = CommonConverter.ToNumber(tarValue);
                var animation = themeControl.ToDoubleAnimation(num, isIn);
                animation.Duration = animation.Duration.TimeSpan / 2;
                animation.Completed += (s, e) =>
                {
                    var locked = tar is Freezable f && f.IsFrozen;
                    if (!locked) tar.SetValue(tarDpProp, num);
                };
                if (tar is UIElement ele) ele.ApplyAnimationClock(tarDpProp, animation.CreateClock());
                else if (tar is Animatable a) a.ApplyAnimationClock(tarDpProp, animation.CreateClock());
                else tar.SetValue(tarDpProp, num);
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
