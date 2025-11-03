using CredVault.Mobile.Models;
using Microsoft.Extensions.Logging;
using Refit;
using System.Net;

namespace CredVault.Mobile.Services;

public class IdentityService
{
    private readonly IIdentityApiClient _apiClient;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(IIdentityApiClient apiClient, ILogger<IdentityService> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Authentication

    /// <summary>
    /// Authenticate user with username and password
    /// </summary>
    public async Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<LoginResponseDto>.Failure("Login request cannot be null");

            _logger.LogInformation("Logging in user: {Username}", request.Username);
            var response = await _apiClient.LoginAsync(request);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully logged in user: {Username}", request.Username);
                return ServiceResult<LoginResponseDto>.Success(response.Data);
            }

            _logger.LogWarning("Failed to login user {Username}: {Message}", request.Username, response.Message);
            return ServiceResult<LoginResponseDto>.Failure(response.Message ?? "Login failed", response.Errors?.ToArray());
        }
        catch (ApiException apiEx) when (apiEx.StatusCode == HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("Invalid credentials for user: {Username}", request.Username);
            return ServiceResult<LoginResponseDto>.Failure("Invalid username or password");
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error during login: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<LoginResponseDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login");
            return ServiceResult<LoginResponseDto>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Logout current user
    /// </summary>
    public async Task<ServiceResult<bool>> LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Logging out user");
            var response = await _apiClient.LogoutAsync();
            
            if (response.Success)
            {
                _logger.LogInformation("Successfully logged out user");
                return ServiceResult<bool>.Success(response.Data);
            }

            _logger.LogWarning("Failed to logout: {Message}", response.Message);
            return ServiceResult<bool>.Failure(response.Message ?? "Logout failed", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error during logout: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<bool>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during logout");
            return ServiceResult<bool>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    public async Task<ServiceResult<RefreshTokenResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<RefreshTokenResponseDto>.Failure("Refresh token request cannot be null");

            _logger.LogInformation("Refreshing access token");
            var response = await _apiClient.RefreshTokenAsync(request);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully refreshed access token");
                return ServiceResult<RefreshTokenResponseDto>.Success(response.Data);
            }

            _logger.LogWarning("Failed to refresh token: {Message}", response.Message);
            return ServiceResult<RefreshTokenResponseDto>.Failure(response.Message ?? "Token refresh failed", response.Errors?.ToArray());
        }
        catch (ApiException apiEx) when (apiEx.StatusCode == HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("Refresh token is invalid or expired");
            return ServiceResult<RefreshTokenResponseDto>.Failure("Session expired. Please login again.");
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error refreshing token: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<RefreshTokenResponseDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error refreshing token");
            return ServiceResult<RefreshTokenResponseDto>.Failure("An unexpected error occurred");
        }
    }

    #endregion

    #region User Registration & Management

    /// <summary>
    /// Register a new user
    /// </summary>
    public async Task<ServiceResult<RegistrationResponseDto>> RegisterAsync(RegistrationRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<RegistrationResponseDto>.Failure("Registration request cannot be null");

            _logger.LogInformation("Registering new user: {Username}", request.Username);
            var response = await _apiClient.RegisterAsync(request);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully registered user: {UserId}", response.Data.UserId);
                return ServiceResult<RegistrationResponseDto>.Success(response.Data);
            }

            _logger.LogWarning("Failed to register user {Username}: {Message}", request.Username, response.Message);
            return ServiceResult<RegistrationResponseDto>.Failure(response.Message ?? "Registration failed", response.Errors?.ToArray());
        }
        catch (ApiException apiEx) when (apiEx.StatusCode == HttpStatusCode.Conflict)
        {
            _logger.LogWarning("User already exists: {Username}", request.Username);
            return ServiceResult<RegistrationResponseDto>.Failure("Username or email already exists");
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error during registration: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<RegistrationResponseDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration");
            return ServiceResult<RegistrationResponseDto>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    public async Task<ServiceResult<UserProfileDto>> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching user profile");
            var response = await _apiClient.GetProfileAsync();
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully fetched user profile: {UserId}", response.Data.UserId);
                return ServiceResult<UserProfileDto>.Success(response.Data);
            }

            _logger.LogWarning("Failed to fetch profile: {Message}", response.Message);
            return ServiceResult<UserProfileDto>.Failure(response.Message ?? "Failed to fetch profile", response.Errors?.ToArray());
        }
        catch (ApiException apiEx) when (apiEx.StatusCode == HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("Unauthorized access to profile");
            return ServiceResult<UserProfileDto>.Failure("Session expired. Please login again.");
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error fetching profile: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<UserProfileDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching profile");
            return ServiceResult<UserProfileDto>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Get user profile by ID
    /// </summary>
    public async Task<ServiceResult<UserProfileDto>> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ServiceResult<UserProfileDto>.Failure("User ID cannot be empty");

            _logger.LogInformation("Fetching user: {UserId}", userId);
            var response = await _apiClient.GetUserByIdAsync(userId);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully fetched user: {UserId}", userId);
                return ServiceResult<UserProfileDto>.Success(response.Data);
            }

            _logger.LogWarning("Failed to fetch user {UserId}: {Message}", userId, response.Message);
            return ServiceResult<UserProfileDto>.Failure(response.Message ?? "User not found", response.Errors?.ToArray());
        }
        catch (ApiException apiEx) when (apiEx.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return ServiceResult<UserProfileDto>.Failure("User not found");
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error fetching user {UserId}: {StatusCode}", userId, apiEx.StatusCode);
            return ServiceResult<UserProfileDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching user {UserId}", userId);
            return ServiceResult<UserProfileDto>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    public async Task<ServiceResult<UserProfileDto>> UpdateProfileAsync(UpdateProfileRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<UserProfileDto>.Failure("Update profile request cannot be null");

            _logger.LogInformation("Updating user profile");
            var response = await _apiClient.UpdateProfileAsync(request);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully updated user profile: {UserId}", response.Data.UserId);
                return ServiceResult<UserProfileDto>.Success(response.Data);
            }

            _logger.LogWarning("Failed to update profile: {Message}", response.Message);
            return ServiceResult<UserProfileDto>.Failure(response.Message ?? "Failed to update profile", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error updating profile: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<UserProfileDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating profile");
            return ServiceResult<UserProfileDto>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Delete user account
    /// </summary>
    public async Task<ServiceResult<bool>> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ServiceResult<bool>.Failure("User ID cannot be empty");

            _logger.LogInformation("Deleting user: {UserId}", userId);
            var response = await _apiClient.DeleteUserAsync(userId);
            
            if (response.Success)
            {
                _logger.LogInformation("Successfully deleted user: {UserId}", userId);
                return ServiceResult<bool>.Success(response.Data);
            }

            _logger.LogWarning("Failed to delete user {UserId}: {Message}", userId, response.Message);
            return ServiceResult<bool>.Failure(response.Message ?? "Failed to delete user", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error deleting user {UserId}: {StatusCode}", userId, apiEx.StatusCode);
            return ServiceResult<bool>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting user {UserId}", userId);
            return ServiceResult<bool>.Failure("An unexpected error occurred");
        }
    }

    #endregion

    #region Password Management

    /// <summary>
    /// Change user password
    /// </summary>
    public async Task<ServiceResult<bool>> ChangePasswordAsync(ChangePasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<bool>.Failure("Change password request cannot be null");

            _logger.LogInformation("Changing user password");
            var response = await _apiClient.ChangePasswordAsync(request);
            
            if (response.Success)
            {
                _logger.LogInformation("Successfully changed password");
                return ServiceResult<bool>.Success(response.Data);
            }

            _logger.LogWarning("Failed to change password: {Message}", response.Message);
            return ServiceResult<bool>.Failure(response.Message ?? "Failed to change password", response.Errors?.ToArray());
        }
        catch (ApiException apiEx) when (apiEx.StatusCode == HttpStatusCode.BadRequest)
        {
            _logger.LogWarning("Invalid password change request");
            return ServiceResult<bool>.Failure("Current password is incorrect");
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error changing password: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<bool>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error changing password");
            return ServiceResult<bool>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Request password reset (forgot password)
    /// </summary>
    public async Task<ServiceResult<bool>> RequestPasswordResetAsync(PasswordResetRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<bool>.Failure("Password reset request cannot be null");

            _logger.LogInformation("Requesting password reset for: {Email}", request.Email);
            var response = await _apiClient.RequestPasswordResetAsync(request);
            
            if (response.Success)
            {
                _logger.LogInformation("Successfully sent password reset email to: {Email}", request.Email);
                return ServiceResult<bool>.Success(response.Data);
            }

            _logger.LogWarning("Failed to request password reset: {Message}", response.Message);
            return ServiceResult<bool>.Failure(response.Message ?? "Failed to request password reset", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error requesting password reset: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<bool>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error requesting password reset");
            return ServiceResult<bool>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Confirm password reset with token
    /// </summary>
    public async Task<ServiceResult<bool>> ConfirmPasswordResetAsync(PasswordResetConfirmDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<bool>.Failure("Password reset confirmation cannot be null");

            _logger.LogInformation("Confirming password reset for: {Email}", request.Email);
            var response = await _apiClient.ConfirmPasswordResetAsync(request);
            
            if (response.Success)
            {
                _logger.LogInformation("Successfully reset password for: {Email}", request.Email);
                return ServiceResult<bool>.Success(response.Data);
            }

            _logger.LogWarning("Failed to confirm password reset: {Message}", response.Message);
            return ServiceResult<bool>.Failure(response.Message ?? "Failed to reset password", response.Errors?.ToArray());
        }
        catch (ApiException apiEx) when (apiEx.StatusCode == HttpStatusCode.BadRequest)
        {
            _logger.LogWarning("Invalid password reset token");
            return ServiceResult<bool>.Failure("Invalid or expired reset token");
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error confirming password reset: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<bool>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error confirming password reset");
            return ServiceResult<bool>.Failure("An unexpected error occurred");
        }
    }

    #endregion

    #region OAuth2/OIDC

    /// <summary>
    /// Create OAuth2 authorization request
    /// </summary>
    public async Task<ServiceResult<OAuth2AuthorizationResponseDto>> AuthorizeAsync(OAuth2AuthorizationRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<OAuth2AuthorizationResponseDto>.Failure("Authorization request cannot be null");

            _logger.LogInformation("Creating OAuth2 authorization for client: {ClientId}", request.ClientId);
            var response = await _apiClient.AuthorizeAsync(request);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully created authorization");
                return ServiceResult<OAuth2AuthorizationResponseDto>.Success(response.Data);
            }

            _logger.LogWarning("Failed to authorize: {Message}", response.Message);
            return ServiceResult<OAuth2AuthorizationResponseDto>.Failure(response.Message ?? "Authorization failed", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error during authorization: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<OAuth2AuthorizationResponseDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authorization");
            return ServiceResult<OAuth2AuthorizationResponseDto>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Exchange authorization code for tokens
    /// </summary>
    public async Task<ServiceResult<OAuth2TokenResponseDto>> GetTokenAsync(OAuth2TokenRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<OAuth2TokenResponseDto>.Failure("Token request cannot be null");

            _logger.LogInformation("Exchanging authorization code for tokens");
            var response = await _apiClient.GetTokenAsync(request);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully obtained tokens");
                return ServiceResult<OAuth2TokenResponseDto>.Success(response.Data);
            }

            _logger.LogWarning("Failed to get tokens: {Message}", response.Message);
            return ServiceResult<OAuth2TokenResponseDto>.Failure(response.Message ?? "Token exchange failed", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error getting tokens: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<OAuth2TokenResponseDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting tokens");
            return ServiceResult<OAuth2TokenResponseDto>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Get OpenID Connect user info
    /// </summary>
    public async Task<ServiceResult<OidcUserInfoDto>> GetUserInfoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching OIDC user info");
            var response = await _apiClient.GetUserInfoAsync();
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully fetched user info: {Sub}", response.Data.Sub);
                return ServiceResult<OidcUserInfoDto>.Success(response.Data);
            }

            _logger.LogWarning("Failed to fetch user info: {Message}", response.Message);
            return ServiceResult<OidcUserInfoDto>.Failure(response.Message ?? "Failed to fetch user info", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error fetching user info: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<OidcUserInfoDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching user info");
            return ServiceResult<OidcUserInfoDto>.Failure("An unexpected error occurred");
        }
    }

    #endregion

    #region Session Management

    /// <summary>
    /// Get current session information
    /// </summary>
    public async Task<ServiceResult<SessionInfoDto>> GetCurrentSessionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching current session");
            var response = await _apiClient.GetCurrentSessionAsync();
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully fetched session: {SessionId}", response.Data.SessionId);
                return ServiceResult<SessionInfoDto>.Success(response.Data);
            }

            _logger.LogWarning("Failed to fetch session: {Message}", response.Message);
            return ServiceResult<SessionInfoDto>.Failure(response.Message ?? "Failed to fetch session", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error fetching session: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<SessionInfoDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching session");
            return ServiceResult<SessionInfoDto>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Get all user sessions
    /// </summary>
    public async Task<ServiceResult<List<SessionInfoDto>>> GetUserSessionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching user sessions");
            var response = await _apiClient.GetUserSessionsAsync();
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully fetched {Count} sessions", response.Data.Count);
                return ServiceResult<List<SessionInfoDto>>.Success(response.Data);
            }

            _logger.LogWarning("Failed to fetch sessions: {Message}", response.Message);
            return ServiceResult<List<SessionInfoDto>>.Failure(response.Message ?? "Failed to fetch sessions", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error fetching sessions: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<List<SessionInfoDto>>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching sessions");
            return ServiceResult<List<SessionInfoDto>>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Revoke a specific session
    /// </summary>
    public async Task<ServiceResult<bool>> RevokeSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                return ServiceResult<bool>.Failure("Session ID cannot be empty");

            _logger.LogInformation("Revoking session: {SessionId}", sessionId);
            var response = await _apiClient.RevokeSessionAsync(sessionId);
            
            if (response.Success)
            {
                _logger.LogInformation("Successfully revoked session: {SessionId}", sessionId);
                return ServiceResult<bool>.Success(response.Data);
            }

            _logger.LogWarning("Failed to revoke session {SessionId}: {Message}", sessionId, response.Message);
            return ServiceResult<bool>.Failure(response.Message ?? "Failed to revoke session", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error revoking session {SessionId}: {StatusCode}", sessionId, apiEx.StatusCode);
            return ServiceResult<bool>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error revoking session {SessionId}", sessionId);
            return ServiceResult<bool>.Failure("An unexpected error occurred");
        }
    }

    #endregion

    #region Verification

    /// <summary>
    /// Request email verification
    /// </summary>
    public async Task<ServiceResult<bool>> RequestEmailVerificationAsync(EmailVerificationRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<bool>.Failure("Email verification request cannot be null");

            _logger.LogInformation("Requesting email verification for: {Email}", request.Email);
            var response = await _apiClient.RequestEmailVerificationAsync(request);
            
            if (response.Success)
            {
                _logger.LogInformation("Successfully sent verification email to: {Email}", request.Email);
                return ServiceResult<bool>.Success(response.Data);
            }

            _logger.LogWarning("Failed to request email verification: {Message}", response.Message);
            return ServiceResult<bool>.Failure(response.Message ?? "Failed to send verification email", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error requesting email verification: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<bool>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error requesting email verification");
            return ServiceResult<bool>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Confirm email verification with code
    /// </summary>
    public async Task<ServiceResult<bool>> ConfirmEmailVerificationAsync(EmailVerificationConfirmDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<bool>.Failure("Email verification confirmation cannot be null");

            _logger.LogInformation("Confirming email verification for: {Email}", request.Email);
            var response = await _apiClient.ConfirmEmailVerificationAsync(request);
            
            if (response.Success)
            {
                _logger.LogInformation("Successfully verified email: {Email}", request.Email);
                return ServiceResult<bool>.Success(response.Data);
            }

            _logger.LogWarning("Failed to verify email: {Message}", response.Message);
            return ServiceResult<bool>.Failure(response.Message ?? "Email verification failed", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error verifying email: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<bool>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error verifying email");
            return ServiceResult<bool>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Request phone verification
    /// </summary>
    public async Task<ServiceResult<bool>> RequestPhoneVerificationAsync(PhoneVerificationRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<bool>.Failure("Phone verification request cannot be null");

            _logger.LogInformation("Requesting phone verification for: {PhoneNumber}", request.PhoneNumber);
            var response = await _apiClient.RequestPhoneVerificationAsync(request);
            
            if (response.Success)
            {
                _logger.LogInformation("Successfully sent verification SMS to: {PhoneNumber}", request.PhoneNumber);
                return ServiceResult<bool>.Success(response.Data);
            }

            _logger.LogWarning("Failed to request phone verification: {Message}", response.Message);
            return ServiceResult<bool>.Failure(response.Message ?? "Failed to send verification SMS", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error requesting phone verification: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<bool>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error requesting phone verification");
            return ServiceResult<bool>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Confirm phone verification with code
    /// </summary>
    public async Task<ServiceResult<bool>> ConfirmPhoneVerificationAsync(PhoneVerificationConfirmDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<bool>.Failure("Phone verification confirmation cannot be null");

            _logger.LogInformation("Confirming phone verification for: {PhoneNumber}", request.PhoneNumber);
            var response = await _apiClient.ConfirmPhoneVerificationAsync(request);
            
            if (response.Success)
            {
                _logger.LogInformation("Successfully verified phone: {PhoneNumber}", request.PhoneNumber);
                return ServiceResult<bool>.Success(response.Data);
            }

            _logger.LogWarning("Failed to verify phone: {Message}", response.Message);
            return ServiceResult<bool>.Failure(response.Message ?? "Phone verification failed", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error verifying phone: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<bool>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error verifying phone");
            return ServiceResult<bool>.Failure("An unexpected error occurred");
        }
    }

    #endregion

    #region Two-Factor Authentication

    /// <summary>
    /// Setup two-factor authentication
    /// </summary>
    public async Task<ServiceResult<TwoFactorSetupResponseDto>> SetupTwoFactorAsync(TwoFactorSetupDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<TwoFactorSetupResponseDto>.Failure("2FA setup request cannot be null");

            _logger.LogInformation("Setting up 2FA: {Method}", request.Method);
            var response = await _apiClient.SetupTwoFactorAsync(request);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully set up 2FA: {Method}", request.Method);
                return ServiceResult<TwoFactorSetupResponseDto>.Success(response.Data);
            }

            _logger.LogWarning("Failed to setup 2FA: {Message}", response.Message);
            return ServiceResult<TwoFactorSetupResponseDto>.Failure(response.Message ?? "2FA setup failed", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error setting up 2FA: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<TwoFactorSetupResponseDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error setting up 2FA");
            return ServiceResult<TwoFactorSetupResponseDto>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Verify two-factor authentication code
    /// </summary>
    public async Task<ServiceResult<bool>> VerifyTwoFactorAsync(TwoFactorVerifyDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<bool>.Failure("2FA verification request cannot be null");

            _logger.LogInformation("Verifying 2FA code");
            var response = await _apiClient.VerifyTwoFactorAsync(request);
            
            if (response.Success)
            {
                _logger.LogInformation("Successfully verified 2FA code");
                return ServiceResult<bool>.Success(response.Data);
            }

            _logger.LogWarning("Failed to verify 2FA code: {Message}", response.Message);
            return ServiceResult<bool>.Failure(response.Message ?? "2FA verification failed", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error verifying 2FA: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<bool>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error verifying 2FA");
            return ServiceResult<bool>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Disable two-factor authentication
    /// </summary>
    public async Task<ServiceResult<bool>> DisableTwoFactorAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Disabling 2FA");
            var response = await _apiClient.DisableTwoFactorAsync();
            
            if (response.Success)
            {
                _logger.LogInformation("Successfully disabled 2FA");
                return ServiceResult<bool>.Success(response.Data);
            }

            _logger.LogWarning("Failed to disable 2FA: {Message}", response.Message);
            return ServiceResult<bool>.Failure(response.Message ?? "Failed to disable 2FA", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error disabling 2FA: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<bool>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error disabling 2FA");
            return ServiceResult<bool>.Failure("An unexpected error occurred");
        }
    }

    #endregion

    #region Health Check

    /// <summary>
    /// Check Identity API health
    /// </summary>
    public async Task<ServiceResult<GovStackHealthResponse>> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Checking Identity API health");
            var response = await _apiClient.GetHealthAsync();
            
            if (response != null)
            {
                _logger.LogInformation("Identity API health status: {Status}", response.Status);
                return ServiceResult<GovStackHealthResponse>.Success(response);
            }

            _logger.LogWarning("Failed to get health status");
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

    #endregion

    #region Helper Methods

    /// <summary>
    /// Extract error details from API exception
    /// </summary>
    private string[]? GetErrorDetails(ApiException apiEx)
    {
        if (string.IsNullOrWhiteSpace(apiEx.Content))
            return null;

        try
        {
            var errorResponse = System.Text.Json.JsonSerializer.Deserialize<GovStackErrorResponse>(apiEx.Content);
            if (errorResponse != null && !string.IsNullOrWhiteSpace(errorResponse.ErrorDescription))
            {
                return new[] { errorResponse.ErrorDescription };
            }
        }
        catch
        {
            return new[] { apiEx.Content };
        }

        return null;
    }

    #endregion
}
