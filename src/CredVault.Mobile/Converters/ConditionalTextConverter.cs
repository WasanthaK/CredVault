using System.Globalization;

namespace CredVault.Mobile.Converters;

public class ConditionalTextConverter : IMultiValueConverter
{
    public object? Convert(object?[]? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 3)
            return string.Empty;

        if (values[0] is bool condition && condition)
        {
            return values[1]?.ToString() ?? string.Empty;
        }
        
        return values[2]?.ToString() ?? string.Empty;
    }

    public object?[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
