using System.Globalization;
using CredVault.Mobile.Models;

namespace CredVault.Mobile.Converters;

public class StatusColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is CredentialStatus status)
        {
            return status switch
            {
                CredentialStatus.Active => Color.FromArgb("#4CAF50"),      // Green
                CredentialStatus.Expired => Color.FromArgb("#F44336"),     // Red
                CredentialStatus.Revoked => Color.FromArgb("#9E9E9E"),     // Gray
                CredentialStatus.Suspended => Color.FromArgb("#FF9800"),   // Orange
                _ => Colors.Gray
            };
        }

        // Fallback for string values
        if (value is string statusString)
        {
            return statusString.ToLowerInvariant() switch
            {
                "active" => Color.FromArgb("#4CAF50"),      // Green
                "expired" => Color.FromArgb("#F44336"),     // Red
                "revoked" => Color.FromArgb("#9E9E9E"),     // Gray
                "suspended" => Color.FromArgb("#FF9800"),   // Orange
                "pending" => Color.FromArgb("#2196F3"),     // Blue
                _ => Colors.Gray
            };
        }

        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
