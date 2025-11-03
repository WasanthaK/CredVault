using CredVault.Mobile.Models;
using Refit;

namespace CredVault.Mobile.Services;

public interface IIdentityApiClient
{
    // Authentication endpoints
    [Post("/api/v1/identity/auth/login")]
    Task<ApiResponseDto<LoginResponseDto>> LoginAsync([Body] LoginRequestDto request);
    
    [Post("/api/v1/identity/auth/logout")]
    Task<BooleanApiResponseDto> LogoutAsync();
    
    [Post("/api/v1/identity/auth/refresh")]
    Task<ApiResponseDto<RefreshTokenResponseDto>> RefreshTokenAsync([Body] RefreshTokenRequestDto request);
    
    // User registration (corrected endpoint)
    [Post("/api/v1/identity/auth/register")]
    Task<ApiResponseDto<RegistrationResponseDto>> RegisterAsync([Body] RegistrationRequestDto request);
    
    // User profile
    [Get("/api/v1/identity/users/profile")]
    Task<ApiResponseDto<UserProfileDto>> GetProfileAsync();
    
    [Get("/api/v1/identity/users/{userId}")]
    Task<ApiResponseDto<UserProfileDto>> GetUserByIdAsync(string userId);
    
    [Put("/api/v1/identity/users/profile")]
    Task<ApiResponseDto<UserProfileDto>> UpdateProfileAsync([Body] UpdateProfileRequestDto request);
    
    [Delete("/api/v1/identity/users/{userId}")]
    Task<BooleanApiResponseDto> DeleteUserAsync(string userId);
    
    // Password management
    [Post("/api/v1/identity/users/change-password")]
    Task<BooleanApiResponseDto> ChangePasswordAsync([Body] ChangePasswordRequestDto request);
    
    [Post("/api/v1/identity/users/reset-password")]
    Task<BooleanApiResponseDto> RequestPasswordResetAsync([Body] PasswordResetRequestDto request);
    
    [Post("/api/v1/identity/users/reset-password/confirm")]
    Task<BooleanApiResponseDto> ConfirmPasswordResetAsync([Body] PasswordResetConfirmDto request);
    
    // OAuth2/OIDC endpoints (corrected paths)
    [Get("/connect/authorize")]
    Task<ApiResponseDto<OAuth2AuthorizationResponseDto>> AuthorizeAsync([Query] OAuth2AuthorizationRequestDto request);
    
    [Post("/connect/token")]
    Task<ApiResponseDto<OAuth2TokenResponseDto>> GetTokenAsync([Body] OAuth2TokenRequestDto request);
    
    [Get("/connect/userinfo")]
    Task<ApiResponseDto<OidcUserInfoDto>> GetUserInfoAsync();
    
    [Post("/connect/revocation")]
    Task<BooleanApiResponseDto> RevokeTokenAsync([Body] TokenRevocationRequestDto request);
    
    // Session management
    [Get("/api/v1/identity/sessions/current")]
    Task<ApiResponseDto<SessionInfoDto>> GetCurrentSessionAsync();
    
    [Get("/api/v1/identity/sessions")]
    Task<ApiResponseDto<List<SessionInfoDto>>> GetUserSessionsAsync();
    
    [Delete("/api/v1/identity/sessions/{sessionId}")]
    Task<BooleanApiResponseDto> RevokeSessionAsync(string sessionId);
    
    // Email verification
    [Post("/api/v1/identity/verification/email/request")]
    Task<BooleanApiResponseDto> RequestEmailVerificationAsync([Body] EmailVerificationRequestDto request);
    
    [Post("/api/v1/identity/verification/email/confirm")]
    Task<BooleanApiResponseDto> ConfirmEmailVerificationAsync([Body] EmailVerificationConfirmDto request);
    
    // Phone verification
    [Post("/api/v1/identity/verification/phone/request")]
    Task<BooleanApiResponseDto> RequestPhoneVerificationAsync([Body] PhoneVerificationRequestDto request);
    
    [Post("/api/v1/identity/verification/phone/confirm")]
    Task<BooleanApiResponseDto> ConfirmPhoneVerificationAsync([Body] PhoneVerificationConfirmDto request);
    
    // Two-factor authentication
    [Post("/api/v1/identity/2fa/setup")]
    Task<ApiResponseDto<TwoFactorSetupResponseDto>> SetupTwoFactorAsync([Body] TwoFactorSetupDto request);
    
    [Post("/api/v1/identity/2fa/verify")]
    Task<BooleanApiResponseDto> VerifyTwoFactorAsync([Body] TwoFactorVerifyDto request);
    
    [Delete("/api/v1/identity/2fa")]
    Task<BooleanApiResponseDto> DisableTwoFactorAsync();
    
    // Health check
    [Get("/api/v1/identity/health")]
    Task<GovStackHealthResponse> GetHealthAsync();
}
