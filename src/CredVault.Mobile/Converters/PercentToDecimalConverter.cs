using System.Globalization;

namespace CredVault.Mobile.Converters;

public class PercentToDecimalConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int percent)
        {
            return percent / 100.0;
        }
        return 0.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double decimal_value)
        {
            return (int)(decimal_value * 100);
        }
        return 0;
    }
}
