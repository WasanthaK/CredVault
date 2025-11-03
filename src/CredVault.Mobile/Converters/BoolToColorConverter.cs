using System.Globalization;

namespace CredVault.Mobile.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isExpired)
        {
            return isExpired ? Color.FromArgb("#F44336") : Color.FromArgb("#4CAF50");
        }

        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
