using System.Globalization;

namespace CredVault.Mobile.Converters;

/// <summary>
/// Converts Material Icon names to their Unicode characters
/// </summary>
public class MaterialIconConverter : IValueConverter
{
    private static readonly Dictionary<string, string> IconMap = new()
    {
        { "person", "\uE7FD" },
        { "cake", "\uE7E9" },
        { "image", "\uE3F4" },
        { "fingerprint", "\uE90D" },
        { "location_on", "\uE0C8" },
        { "email", "\uE0BE" },
        { "phone", "\uE0CD" },
        { "home", "\uE88A" },
        { "work", "\uE8F9" },
        { "badge", "\uEA67" },
        { "verified_user", "\uE8EE" },
        { "check_circle", "\uE86C" },
        { "arrow_back", "\uE5C4" },
        { "help_outline", "\uE887" }
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string iconName && IconMap.TryGetValue(iconName, out var unicode))
        {
            return unicode;
        }
        return "\uE3F4"; // Default to image icon
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
