using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace CredVault.Mobile.Services;

/// <summary>
/// HTTP message handler that automatically adds Bearer token authorization header to all requests.
/// Retrieves access token from SecureStorage and injects it into the Authorization header.
/// </summary>
public class AuthHeaderHandler : DelegatingHandler
{
    private readonly ILogger<AuthHeaderHandler> _logger;

    public AuthHeaderHandler(ILogger<AuthHeaderHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = await SecureStorage.GetAsync("access_token");
            
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogDebug("Added Bearer token to request: {Method} {Url}", 
                    request.Method, request.RequestUri);
            }
            else
            {
                _logger.LogDebug("No access token found for request: {Method} {Url}", 
                    request.Method, request.RequestUri);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving or adding auth header for request: {Url}", 
                request.RequestUri);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
