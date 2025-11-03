using System.Globalization;

namespace CredVault.Mobile.Converters;

public class PinDotColorConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 4)
            return Colors.Gray;

        try
        {
            var pinLength = values[0] as int? ?? 0;
            var confirmPinLength = values[1] as int? ?? 0;
            var isConfirmStep = values[2] as bool? ?? false;
            var dotIndex = values[3] as int? ?? 0;

            var currentLength = isConfirmStep ? confirmPinLength : pinLength;

            // If this dot position is filled, return primary color
            if (dotIndex < currentLength)
            {
                return Color.FromArgb("#004aad");
            }

            // Otherwise return gray
            return Application.Current?.RequestedTheme == AppTheme.Dark 
                ? Color.FromArgb("#555555") 
                : Color.FromArgb("#dddddd");
        }
        catch
        {
            return Colors.Gray;
        }
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
