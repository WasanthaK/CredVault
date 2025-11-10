namespace CredVault.Mobile.Models;

/// <summary>
/// Login request DTO
/// </summary>
public class LoginRequestDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

/// <summary>
/// Login response DTO with tokens
/// </summary>
public class LoginResponseDto
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public required string TokenType { get; set; }
    public int ExpiresIn { get; set; }
    public string? Scope { get; set; }
    public string? IdToken { get; set; }
    public UserProfileDto? User { get; set; }
}

/// <summary>
/// Token refresh request
/// </summary>
public class RefreshTokenRequestDto
{
    public required string RefreshToken { get; set; }
    public string? ClientId { get; set; }
}

/// <summary>
/// Token refresh response
/// </summary>
public class RefreshTokenResponseDto
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public required string TokenType { get; set; }
    public int ExpiresIn { get; set; }
    public string? Scope { get; set; }
}

/// <summary>
/// User registration request
/// </summary>
public class RegistrationRequestDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Wrapper for registration request (Azure API expects this format)
/// </summary>
public class RegistrationRequestWrapper
{
    public required RegistrationRequestDto Request { get; set; }
}

/// <summary>
/// User registration response
/// </summary>
public class RegistrationResponseDto
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public List<string>? Roles { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int LoginCount { get; set; }
}

/// <summary>
/// User profile DTO
/// </summary>
public class UserProfileDto
{
    public required string Id { get; set; }
    public string UserId => Id; // Alias for backwards compatibility
    public string Username => Email; // Use email as username
    public required string Email { get; set; }
    public bool IsEmailVerified { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsPhoneVerified { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Country { get; set; }
    public string? Language { get; set; }
    public string? TimeZone { get; set; }
    public List<string>? Roles { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int LoginCount { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Update user profile request
/// </summary>
public class UpdateProfileRequestDto
{
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Country { get; set; }
    public string? Language { get; set; }
    public string? TimeZone { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Change password request
/// </summary>
public class ChangePasswordRequestDto
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}

/// <summary>
/// Password reset request (forgot password)
/// </summary>
public class PasswordResetRequestDto
{
    public required string Email { get; set; }
}

/// <summary>
/// Password reset confirmation
/// </summary>
public class PasswordResetConfirmDto
{
    public required string Email { get; set; }
    public required string ResetToken { get; set; }
    public required string NewPassword { get; set; }
}

/// <summary>
/// OAuth2 authorization request
/// </summary>
public class OAuth2AuthorizationRequestDto
{
    public required string ResponseType { get; set; }
    public required string ClientId { get; set; }
    public required string RedirectUri { get; set; }
    public string? Scope { get; set; }
    public string? State { get; set; }
    public string? CodeChallenge { get; set; }
    public string? CodeChallengeMethod { get; set; }
}

/// <summary>
/// OAuth2 authorization response
/// </summary>
public class OAuth2AuthorizationResponseDto
{
    public required string Code { get; set; }
    public string? State { get; set; }
}

/// <summary>
/// OAuth2 token request (authorization code flow)
/// </summary>
public class OAuth2TokenRequestDto
{
    public required string GrantType { get; set; }
    public required string Code { get; set; }
    public required string RedirectUri { get; set; }
    public required string ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? CodeVerifier { get; set; }
}

/// <summary>
/// OAuth2 token response
/// </summary>
public class OAuth2TokenResponseDto
{
    public required string AccessToken { get; set; }
    public required string TokenType { get; set; }
    public int ExpiresIn { get; set; }
    public string? RefreshToken { get; set; }
    public string? Scope { get; set; }
    public string? IdToken { get; set; }
}

/// <summary>
/// OpenID Connect UserInfo response
/// </summary>
public class OidcUserInfoDto
{
    public required string Sub { get; set; }
    public string? Name { get; set; }
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
    public string? MiddleName { get; set; }
    public string? Nickname { get; set; }
    public string? PreferredUsername { get; set; }
    public string? Profile { get; set; }
    public string? Picture { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
    public bool? EmailVerified { get; set; }
    public string? Gender { get; set; }
    public string? Birthdate { get; set; }
    public string? Zoneinfo { get; set; }
    public string? Locale { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? PhoneNumberVerified { get; set; }
    public Dictionary<string, object>? Address { get; set; }
    public long? UpdatedAt { get; set; }
}

/// <summary>
/// Session information
/// </summary>
public class SessionInfoDto
{
    public required string SessionId { get; set; }
    public required string UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Email verification request
/// </summary>
public class EmailVerificationRequestDto
{
    public required string Email { get; set; }
}

/// <summary>
/// Email verification confirmation
/// </summary>
public class EmailVerificationConfirmDto
{
    public required string Email { get; set; }
    public required string VerificationCode { get; set; }
}

/// <summary>
/// Phone verification request
/// </summary>
public class PhoneVerificationRequestDto
{
    public required string PhoneNumber { get; set; }
}

/// <summary>
/// Phone verification confirmation
/// </summary>
public class PhoneVerificationConfirmDto
{
    public required string PhoneNumber { get; set; }
    public required string VerificationCode { get; set; }
}

/// <summary>
/// Two-factor authentication setup request
/// </summary>
public class TwoFactorSetupDto
{
    public required string Method { get; set; } // "totp", "sms", "email"
}

/// <summary>
/// Two-factor authentication setup response
/// </summary>
public class TwoFactorSetupResponseDto
{
    public required string Method { get; set; }
    public string? Secret { get; set; }
    public string? QrCodeUrl { get; set; }
    public List<string>? BackupCodes { get; set; }
}

/// <summary>
/// Two-factor authentication verification
/// </summary>
public class TwoFactorVerifyDto
{
    public required string Code { get; set; }
    public string? Method { get; set; }
}

/// <summary>
/// Token revocation request (RFC 7009)
/// </summary>
public class TokenRevocationRequestDto
{
    public required string Token { get; set; }
    public string? TokenTypeHint { get; set; } // "access_token" or "refresh_token"
}
