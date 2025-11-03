using CredVault.Mobile.Models;
using Microsoft.Extensions.Logging;
using Refit;
using System.Net;

namespace CredVault.Mobile.Services;

public class ConsentService
{
    private readonly IConsentApiClient _apiClient;
    private readonly ILogger<ConsentService> _logger;

    public ConsentService(IConsentApiClient apiClient, ILogger<ConsentService> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ServiceResult<ConsentResponseDto>> CreateConsentAsync(ConsentRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<ConsentResponseDto>.Failure("Consent request cannot be null");

            _logger.LogInformation("Creating consent for user: {UserId}, client: {ClientId}", request.UserId, request.ClientId);
            var response = await _apiClient.CreateConsentAsync(request);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully created consent: {ConsentId}", response.Data.ConsentId);
                return ServiceResult<ConsentResponseDto>.Success(response.Data);
            }

            return ServiceResult<ConsentResponseDto>.Failure(response.Message ?? "Failed to create consent", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error creating consent: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<ConsentResponseDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating consent");
            return ServiceResult<ConsentResponseDto>.Failure("An unexpected error occurred");
        }
    }

    public async Task<ServiceResult<ConsentResponseDto>> GetConsentByIdAsync(string consentId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(consentId))
                return ServiceResult<ConsentResponseDto>.Failure("Consent ID cannot be empty");

            var response = await _apiClient.GetConsentByIdAsync(consentId);
            
            if (response.Success && response.Data != null)
                return ServiceResult<ConsentResponseDto>.Success(response.Data);

            return ServiceResult<ConsentResponseDto>.Failure(response.Message ?? "Consent not found", response.Errors?.ToArray());
        }
        catch (ApiException apiEx) when (apiEx.StatusCode == HttpStatusCode.NotFound)
        {
            return ServiceResult<ConsentResponseDto>.Failure("Consent not found");
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error fetching consent: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<ConsentResponseDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching consent");
            return ServiceResult<ConsentResponseDto>.Failure("An unexpected error occurred");
        }
    }

    public async Task<ServiceResult<List<ConsentResponseDto>>> GetConsentsAsync(ConsentQueryDto? query = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.GetConsentsAsync(query);
            
            if (response.Success && response.Data != null)
                return ServiceResult<List<ConsentResponseDto>>.Success(response.Data);

            return ServiceResult<List<ConsentResponseDto>>.Failure(response.Message ?? "Failed to fetch consents", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error fetching consents: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<List<ConsentResponseDto>>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching consents");
            return ServiceResult<List<ConsentResponseDto>>.Failure("An unexpected error occurred");
        }
    }

    public async Task<ServiceResult<ConsentResponseDto>> UpdateConsentAsync(string consentId, ConsentUpdateDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(consentId))
                return ServiceResult<ConsentResponseDto>.Failure("Consent ID cannot be empty");
            if (request == null)
                return ServiceResult<ConsentResponseDto>.Failure("Update request cannot be null");

            var response = await _apiClient.UpdateConsentAsync(consentId, request);
            
            if (response.Success && response.Data != null)
                return ServiceResult<ConsentResponseDto>.Success(response.Data);

            return ServiceResult<ConsentResponseDto>.Failure(response.Message ?? "Failed to update consent", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error updating consent: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<ConsentResponseDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating consent");
            return ServiceResult<ConsentResponseDto>.Failure("An unexpected error occurred");
        }
    }

    public async Task<ServiceResult<bool>> RevokeConsentAsync(string consentId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(consentId))
                return ServiceResult<bool>.Failure("Consent ID cannot be empty");

            _logger.LogInformation("Revoking consent: {ConsentId}", consentId);
            var response = await _apiClient.RevokeConsentAsync(consentId);
            
            if (response.Success)
                return ServiceResult<bool>.Success(response.Data);

            return ServiceResult<bool>.Failure(response.Message ?? "Failed to revoke consent", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error revoking consent: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<bool>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error revoking consent");
            return ServiceResult<bool>.Failure("An unexpected error occurred");
        }
    }

    public async Task<ServiceResult<BulkConsentRevocationResponseDto>> RevokeBulkConsentsAsync(BulkConsentRevocationDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null || request.ConsentIds == null || !request.ConsentIds.Any())
                return ServiceResult<BulkConsentRevocationResponseDto>.Failure("Consent IDs cannot be empty");

            _logger.LogInformation("Revoking {Count} consents", request.ConsentIds.Count);
            var response = await _apiClient.RevokeBulkConsentsAsync(request);
            
            if (response.Success && response.Data != null)
                return ServiceResult<BulkConsentRevocationResponseDto>.Success(response.Data);

            return ServiceResult<BulkConsentRevocationResponseDto>.Failure(response.Message ?? "Failed to revoke consents", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error revoking bulk consents: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<BulkConsentRevocationResponseDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error revoking bulk consents");
            return ServiceResult<BulkConsentRevocationResponseDto>.Failure("An unexpected error occurred");
        }
    }

    public async Task<ServiceResult<ConsentVerificationResponseDto>> VerifyConsentAsync(ConsentVerificationDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<ConsentVerificationResponseDto>.Failure("Verification request cannot be null");

            var response = await _apiClient.VerifyConsentAsync(request);
            
            if (response.Success && response.Data != null)
                return ServiceResult<ConsentVerificationResponseDto>.Success(response.Data);

            return ServiceResult<ConsentVerificationResponseDto>.Failure(response.Message ?? "Failed to verify consent", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error verifying consent: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<ConsentVerificationResponseDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error verifying consent");
            return ServiceResult<ConsentVerificationResponseDto>.Failure("An unexpected error occurred");
        }
    }

    public async Task<ServiceResult<List<DataScopeDto>>> GetAvailableScopesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.GetAvailableScopesAsync();
            
            if (response.Success && response.Data != null)
                return ServiceResult<List<DataScopeDto>>.Success(response.Data);

            return ServiceResult<List<DataScopeDto>>.Failure(response.Message ?? "Failed to fetch scopes", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error fetching scopes: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<List<DataScopeDto>>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching scopes");
            return ServiceResult<List<DataScopeDto>>.Failure("An unexpected error occurred");
        }
    }

    public async Task<ServiceResult<List<ConsentResponseDto>>> GetUserConsentsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ServiceResult<List<ConsentResponseDto>>.Failure("User ID cannot be empty");

            var response = await _apiClient.GetUserConsentsAsync(userId);
            
            if (response.Success && response.Data != null)
                return ServiceResult<List<ConsentResponseDto>>.Success(response.Data);

            return ServiceResult<List<ConsentResponseDto>>.Failure(response.Message ?? "Failed to fetch user consents", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error fetching user consents: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<List<ConsentResponseDto>>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching user consents");
            return ServiceResult<List<ConsentResponseDto>>.Failure("An unexpected error occurred");
        }
    }

    public async Task<ServiceResult<ConsentStatisticsDto>> GetUserConsentStatisticsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ServiceResult<ConsentStatisticsDto>.Failure("User ID cannot be empty");

            var response = await _apiClient.GetUserConsentStatisticsAsync(userId);
            
            if (response.Success && response.Data != null)
                return ServiceResult<ConsentStatisticsDto>.Success(response.Data);

            return ServiceResult<ConsentStatisticsDto>.Failure(response.Message ?? "Failed to fetch statistics", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error fetching statistics: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<ConsentStatisticsDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching statistics");
            return ServiceResult<ConsentStatisticsDto>.Failure("An unexpected error occurred");
        }
    }

    public async Task<ServiceResult<GovStackHealthResponse>> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.GetHealthAsync();
            
            if (response != null)
                return ServiceResult<GovStackHealthResponse>.Success(response);

            return ServiceResult<GovStackHealthResponse>.Failure("Failed to get health status");
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error checking health: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<GovStackHealthResponse>.Failure($"API error: {apiEx.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error checking health");
            return ServiceResult<GovStackHealthResponse>.Failure("An unexpected error occurred");
        }
    }

    private string[]? GetErrorDetails(ApiException apiEx)
    {
        if (string.IsNullOrWhiteSpace(apiEx.Content))
            return null;

        try
        {
            var errorResponse = System.Text.Json.JsonSerializer.Deserialize<GovStackErrorResponse>(apiEx.Content);
            if (errorResponse != null && !string.IsNullOrWhiteSpace(errorResponse.ErrorDescription))
                return new[] { errorResponse.ErrorDescription };
        }
        catch
        {
            return new[] { apiEx.Content };
        }

        return null;
    }
}
