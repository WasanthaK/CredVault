using System.Globalization;

namespace CredVault.Mobile.Converters;

public class ConditionalColorConverter : IMultiValueConverter
{
    public object? Convert(object?[]? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 3)
            return Colors.Transparent;

        if (values[0] is bool condition && condition)
        {
            return ParseColor(values[1]);
        }
        
        return ParseColor(values[2]);
    }

    public object?[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private Color ParseColor(object? value)
    {
        if (value is Color color)
            return color;
            
        if (value is string colorString)
        {
            try
            {
                return Color.FromArgb(colorString);
            }
            catch
            {
                return Colors.Transparent;
            }
        }
        
        return Colors.Transparent;
    }
}
