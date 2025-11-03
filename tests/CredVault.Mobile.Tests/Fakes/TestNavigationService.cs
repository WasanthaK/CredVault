using CredVault.Mobile.Services;

namespace CredVault.Mobile.Tests.Fakes;

internal sealed class TestNavigationService : INavigationService
{
    public List<string> Routes { get; } = new();
    public List<IDictionary<string, object>> Parameters { get; } = new();
    public int GoBackCount { get; private set; }

    public Task NavigateToAsync(string route, bool animate = true)
    {
        Routes.Add(route);
        Parameters.Add(new Dictionary<string, object>());
        return Task.CompletedTask;
    }

    public Task NavigateToAsync(string route, IDictionary<string, object> parameters, bool animate = true)
    {
        Routes.Add(route);
        Parameters.Add(new Dictionary<string, object>(parameters));
        return Task.CompletedTask;
    }

    public Task GoBackAsync(bool animate = true)
    {
        GoBackCount++;
        return Task.CompletedTask;
    }

    public Task GoToRootAsync(bool animate = true)
    {
        Routes.Add("__root");
        Parameters.Add(new Dictionary<string, object>());
        return Task.CompletedTask;
    }

    public Task ShowModalAsync(string route, IDictionary<string, object>? parameters = null, bool animate = true)
    {
        Routes.Add($"modal:{route}");
        Parameters.Add(new Dictionary<string, object>(parameters ?? new Dictionary<string, object>()));
        return Task.CompletedTask;
    }

    public Task CloseModalAsync(bool animate = true)
    {
        Routes.Add("modal:close");
        Parameters.Add(new Dictionary<string, object>());
        return Task.CompletedTask;
    }

    public int GetNavigationStackCount() => Routes.Count;
}
