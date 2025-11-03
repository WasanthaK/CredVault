using Microsoft.Extensions.Logging;

namespace CredVault.Mobile.Services;

public class NavigationService : INavigationService
{
    private readonly ILogger<NavigationService> _logger;

    public NavigationService(ILogger<NavigationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private INavigation Navigation
    {
        get
        {
            var window = Application.Current?.Windows.FirstOrDefault();
            var navigation = window?.Page?.Navigation;
            if (navigation == null)
            {
                _logger.LogError("Navigation is null - Window Page may not be initialized");
                throw new InvalidOperationException("Navigation is not available. Ensure Window Page is set.");
            }
            return navigation;
        }
    }

    public async Task NavigateToAsync(string route, bool animate = true)
    {
        await NavigateToAsync(route, new Dictionary<string, object>(), animate);
    }

    public async Task NavigateToAsync(string route, IDictionary<string, object> parameters, bool animate = true)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(route))
                throw new ArgumentException("Route cannot be null or empty", nameof(route));

            _logger.LogInformation("Navigating to route: {Route}", route);

            // Use Shell navigation if available
            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync(route, animate, parameters);
            }
            else
            {
                // Fallback to traditional navigation
                // Note: This requires pages to be registered in IoC container
                _logger.LogWarning("Shell navigation not available, using fallback navigation");
                throw new NotImplementedException("Non-Shell navigation requires page resolution from DI container");
            }

            _logger.LogDebug("Navigation to {Route} completed", route);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to route: {Route}", route);
            throw;
        }
    }

    public async Task GoBackAsync(bool animate = true)
    {
        try
        {
            _logger.LogInformation("Navigating back");

            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync("..", animate);
            }
            else if (Navigation.NavigationStack.Count > 1)
            {
                await Navigation.PopAsync(animate);
            }
            else
            {
                _logger.LogWarning("Cannot navigate back - no pages in stack");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating back");
            throw;
        }
    }

    public async Task GoToRootAsync(bool animate = true)
    {
        try
        {
            _logger.LogInformation("Navigating to root");

            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync("///", animate);
            }
            else
            {
                await Navigation.PopToRootAsync(animate);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to root");
            throw;
        }
    }

    public async Task ShowModalAsync(string route, IDictionary<string, object>? parameters = null, bool animate = true)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(route))
                throw new ArgumentException("Route cannot be null or empty", nameof(route));

            _logger.LogInformation("Showing modal: {Route}", route);

            // Modals in Shell require specific routing patterns
            if (Shell.Current != null)
            {
                var modalRoute = route.StartsWith("//") ? route : $"//{route}";
                await Shell.Current.GoToAsync(modalRoute, animate, parameters ?? new Dictionary<string, object>());
            }
            else
            {
                _logger.LogWarning("Shell navigation not available for modal");
                throw new NotImplementedException("Non-Shell modal navigation requires page resolution from DI container");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing modal: {Route}", route);
            throw;
        }
    }

    public async Task CloseModalAsync(bool animate = true)
    {
        try
        {
            _logger.LogInformation("Closing modal");

            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync("..", animate);
            }
            else if (Navigation.ModalStack.Count > 0)
            {
                await Navigation.PopModalAsync(animate);
            }
            else
            {
                _logger.LogWarning("No modal to close");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing modal");
            throw;
        }
    }

    public int GetNavigationStackCount()
    {
        if (Shell.Current != null)
        {
            return Shell.Current.Navigation.NavigationStack.Count;
        }
        return Navigation?.NavigationStack.Count ?? 0;
    }
}
