using ElShrine.Wpf.Controls;
using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace ElShrine.Wpf.Converters;

[ValueConversion(typeof(HeaderPlacement), typeof(double))]
[ValueConversion(typeof(ExpandDirection), typeof(double))]
public class PlacementAngleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var angle = 0d;
        if (value is HeaderPlacement placement)
        {
            angle = placement switch
            {
                HeaderPlacement.Bottom => 0d,
                HeaderPlacement.LeftBottom => 45d,
                HeaderPlacement.Left => 90d,
                HeaderPlacement.LeftTop => 135d,
                HeaderPlacement.Top => 180d,
                HeaderPlacement.RightTop => 225d,
                HeaderPlacement.Right => 270d,
                HeaderPlacement.RightBottom => 315d,
                _ => 0d
            };
        }
        else if (value is ExpandDirection direction)
        {
            angle = direction switch
            {
                ExpandDirection.Down => 0d,
                ExpandDirection.Up => 180d,
                ExpandDirection.Left => 90d,
                ExpandDirection.Right => 270d,
                _ => 0d
            };
        }
        angle = CommonModifyConverter.DoParameterCalculation(angle, parameter?.ToString());
        var ra = angle % 360d;
        return ra < 0 ? ra + 360d : ra;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
