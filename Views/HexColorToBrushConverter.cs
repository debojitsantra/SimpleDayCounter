using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SimpleDayCounter.Views
{
    /// <summary>
    /// Converts a hex color string  into a SolidColorBrush
    /// for use in data binding, since WPF has no implicit string-to-Brush
    /// conversion in the binding pipeline.
    /// </summary>
    public class HexColorToBrushConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string hex)
            {
                try
                {
                    var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);
                    return new SolidColorBrush(color);
                }
                catch
                {
                   
                }
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
