using CredVault.Mobile.Models;
using Refit;

namespace CredVault.Mobile.Services;

public interface IConsentApiClient
{
    // Consent management
    [Post("/api/v1/consent")]
    Task<ApiResponseDto<ConsentResponseDto>> CreateConsentAsync([Body] ConsentRequestDto request);
    
    [Get("/api/v1/consent/{consentId}")]
    Task<ApiResponseDto<ConsentResponseDto>> GetConsentByIdAsync(string consentId);
    
    [Get("/api/v1/consent")]
    Task<ApiResponseDto<List<ConsentResponseDto>>> GetConsentsAsync([Query] ConsentQueryDto? query = null);
    
    [Put("/api/v1/consent/{consentId}")]
    Task<ApiResponseDto<ConsentResponseDto>> UpdateConsentAsync(string consentId, [Body] ConsentUpdateDto request);
    
    [Delete("/api/v1/consent/{consentId}")]
    Task<BooleanApiResponseDto> RevokeConsentAsync(string consentId);
    
    [Post("/api/v1/consent/revoke-bulk")]
    Task<ApiResponseDto<BulkConsentRevocationResponseDto>> RevokeBulkConsentsAsync([Body] BulkConsentRevocationDto request);
    
    // Consent verification
    [Post("/api/v1/consent/verify")]
    Task<ApiResponseDto<ConsentVerificationResponseDto>> VerifyConsentAsync([Body] ConsentVerificationDto request);
    
    // Data scopes
    [Get("/api/v1/consent/scopes")]
    Task<ApiResponseDto<List<DataScopeDto>>> GetAvailableScopesAsync();
    
    [Get("/api/v1/consent/scopes/{scopeId}")]
    Task<ApiResponseDto<DataScopeDto>> GetScopeByIdAsync(string scopeId);
    
    // User consent management
    [Get("/api/v1/consent/user/{userId}")]
    Task<ApiResponseDto<List<ConsentResponseDto>>> GetUserConsentsAsync(string userId);
    
    [Get("/api/v1/consent/user/{userId}/statistics")]
    Task<ApiResponseDto<ConsentStatisticsDto>> GetUserConsentStatisticsAsync(string userId);
    
    [Delete("/api/v1/consent/user/{userId}/revoke-all")]
    Task<BooleanApiResponseDto> RevokeAllUserConsentsAsync(string userId);
    
    // Client consent management
    [Get("/api/v1/consent/client/{clientId}")]
    Task<ApiResponseDto<List<ConsentResponseDto>>> GetClientConsentsAsync(string clientId);
    
    // Health check
    [Get("/api/v1/consent/health")]
    Task<GovStackHealthResponse> GetHealthAsync();
}
