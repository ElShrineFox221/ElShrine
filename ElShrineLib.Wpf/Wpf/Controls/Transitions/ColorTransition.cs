using ElShrine.Wpf.Controls.Extensions;
using ElShrine.Wpf.Converters;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ElShrine.Wpf.Controls.Transitions
{
    public class ColorTransition : ITransition
    {
        public bool TryDoTransition(DependencyObject tar, DependencyProperty tarDpProp, object? tarValue, bool isIn)
        {
            try
            {
                var oldTar = tar;
                var oldDpProp = tarDpProp;
                if (tarDpProp.PropertyType == typeof(Brush))
                {
                    var newtar = (DependencyObject)tar.GetValue(tarDpProp);
                    if (newtar is null) tar.SetValue(tarDpProp, newtar = new SolidColorBrush());
                    if (newtar is not SolidColorBrush) return false;
                    tar = newtar;
                    tarDpProp = SolidColorBrush.ColorProperty;
                }
                else if (tarDpProp.PropertyType != typeof(Color)) return false;
                TransHelper.GetThemeControlParent(tar, out _, out var themeControl);
                var colorData = CommonConverter.ToColor(tarValue);
                var mediaColor = colorData.ToMediaColor();
                var colorAnimation = new ColorAnimation()
                {
                    To = mediaColor,
                    Duration = TimeSpan.FromSeconds((isIn ? themeControl?.AnimaDurationIn : themeControl?.AnimaDurationOut) ?? Constants.DefaultAnimationDuration),
                    EasingFunction = themeControl?.AnimaEaseFunc,
                    FillBehavior = FillBehavior.HoldEnd
                };
                colorAnimation.Completed += (s, e) =>
                {
                    var locked = tar is Freezable f && f.IsFrozen;
                    if (!locked) tar.SetValue(tarDpProp, mediaColor);
                    else oldTar.SetValue(oldDpProp, mediaColor.ToSolidBrush());
                };
                if (tar is UIElement ele) ele.ApplyAnimationClock(tarDpProp, colorAnimation.CreateClock());
                else if(tar is Animatable a) a.ApplyAnimationClock(tarDpProp, colorAnimation.CreateClock());
                else tar.SetValue(tarDpProp, mediaColor);
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
