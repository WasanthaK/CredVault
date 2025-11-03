using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace CredVault.Mobile.Helpers;

public static class ServiceHelper
{
    public static T GetRequiredService<T>() where T : notnull
    {
        var services = GetServiceProvider();
        return services.GetRequiredService<T>();
    }

    private static IServiceProvider GetServiceProvider()
    {
        if (Application.Current?.Handler?.MauiContext?.Services is IServiceProvider handlerServices)
        {
            return handlerServices;
        }

        throw new InvalidOperationException("Service provider is not available yet.");
    }
}
