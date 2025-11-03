namespace CredVault.Mobile.Services;

/// <summary>
/// Navigation service interface for page navigation
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigate to a page by route
    /// </summary>
    Task NavigateToAsync(string route, bool animate = true);

    /// <summary>
    /// Navigate to a page with parameters
    /// </summary>
    Task NavigateToAsync(string route, IDictionary<string, object> parameters, bool animate = true);

    /// <summary>
    /// Navigate back
    /// </summary>
    Task GoBackAsync(bool animate = true);

    /// <summary>
    /// Navigate to root (clear stack)
    /// </summary>
    Task GoToRootAsync(bool animate = true);

    /// <summary>
    /// Show modal page
    /// </summary>
    Task ShowModalAsync(string route, IDictionary<string, object>? parameters = null, bool animate = true);

    /// <summary>
    /// Close modal page
    /// </summary>
    Task CloseModalAsync(bool animate = true);

    /// <summary>
    /// Get current navigation stack depth
    /// </summary>
    int GetNavigationStackCount();
}
