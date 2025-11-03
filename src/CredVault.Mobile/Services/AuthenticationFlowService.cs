using CredVault.Mobile.Models;
using Microsoft.Extensions.Logging;

namespace CredVault.Mobile.Services;

/// <summary>
/// Service for handling OAuth 2.0 and OpenID4VCI authentication flows
/// </summary>
public class AuthenticationFlowService
{
    private readonly IdentityService _identityService;
    private readonly WalletService _walletService;
    private readonly ILogger<AuthenticationFlowService> _logger;
    private readonly ISecureStorage _secureStorage;

    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";
    private const string TokenExpiryKey = "token_expiry";

    public AuthenticationFlowService(
        IdentityService identityService,
        WalletService walletService,
        ISecureStorage secureStorage,
        ILogger<AuthenticationFlowService> logger)
    {
        _identityService = identityService;
        _walletService = walletService;
        _secureStorage = secureStorage;
        _logger = logger;
    }

    /// <summary>
    /// Start the credential issuance flow with OpenID4VCI
    /// Returns the authorization URL that should be opened in a browser
    /// </summary>
    public async Task<ServiceResult<string>> StartCredentialIssuanceFlowAsync(string credentialType, string issuerId)
    {
        try
        {
            _logger.LogInformation("Starting credential issuance flow for type: {CredentialType}, issuer: {IssuerId}", 
                credentialType, issuerId);

            // Step 1: Get credential offer from the issuer
            // In a real implementation, this would come from a QR code scan or deep link
            var credentialOfferUrl = BuildCredentialOfferUrl(credentialType, issuerId);

            // Step 2: Parse the credential offer to get the issuer's metadata
            var issuerMetadata = await GetIssuerMetadataAsync(issuerId);
            if (!issuerMetadata.IsSuccess || issuerMetadata.Data is null)
            {
                return ServiceResult<string>.Failure(
                    issuerMetadata.ErrorMessage ?? "Failed to get issuer metadata",
                    issuerMetadata.ErrorDetails);
            }

            // Step 3: Build the authorization URL for the identity provider
            var authUrl = BuildAuthorizationUrl(issuerMetadata.Data, credentialType);

            _logger.LogInformation("Authorization URL created: {AuthUrl}", authUrl);
            return ServiceResult<string>.Success(authUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting credential issuance flow");
            return ServiceResult<string>.Failure($"Failed to start credential issuance: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle the OAuth callback after user authenticates with identity provider
    /// </summary>
    public async Task<ServiceResult<string>> HandleOAuthCallbackAsync(string authorizationCode, string state)
    {
        try
        {
            _logger.LogInformation("Handling OAuth callback with authorization code");

            // Step 1: Exchange authorization code for access token
            var tokenResult = await ExchangeCodeForTokenAsync(authorizationCode);
            if (!tokenResult.IsSuccess || tokenResult.Data is null)
            {
                return ServiceResult<string>.Failure(
                    tokenResult.ErrorMessage ?? "Failed to exchange authorization code for token",
                    tokenResult.ErrorDetails);
            }

            // Step 2: Store tokens securely
            await StoreTokensAsync(tokenResult.Data);

            _logger.LogInformation("OAuth callback handled successfully");
            return ServiceResult<string>.Success(tokenResult.Data.AccessToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling OAuth callback");
            return ServiceResult<string>.Failure($"Failed to handle OAuth callback: {ex.Message}");
        }
    }

    /// <summary>
    /// Request credential issuance after authentication
    /// </summary>
    public async Task<ServiceResult<CredentialOfferDetails>> RequestCredentialIssuanceAsync(
        string credentialType, 
        string issuerId)
    {
        try
        {
            _logger.LogInformation("Requesting credential issuance for type: {CredentialType}", credentialType);

            // Step 1: Get stored access token
            var accessToken = await _secureStorage.GetAsync(AccessTokenKey);
            if (string.IsNullOrEmpty(accessToken))
            {
                return ServiceResult<CredentialOfferDetails>.Failure("No access token available. Please authenticate first.");
            }

            // Step 2: Request credential from issuer using the access token
            // This would call the issuer's credential endpoint
            var credentialOffer = await GetCredentialOfferDetailsAsync(credentialType, issuerId, accessToken);

            return credentialOffer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting credential issuance");
            return ServiceResult<CredentialOfferDetails>.Failure($"Failed to request credential: {ex.Message}");
        }
    }

    /// <summary>
    /// Accept credential and store in wallet after user consent
    /// </summary>
    public async Task<ServiceResult<CredentialResponseDto>> AcceptAndStoreCredentialAsync(
        CredentialOfferDetails offerDetails, 
        bool userConsented)
    {
        try
        {
            if (!userConsented)
            {
                return ServiceResult<CredentialResponseDto>.Failure("User did not consent to credential issuance");
            }

            _logger.LogInformation("Accepting and storing credential: {CredentialType}", offerDetails.CredentialType);

            // Step 1: Get access token
            var accessToken = await _secureStorage.GetAsync(AccessTokenKey);
            if (string.IsNullOrEmpty(accessToken))
            {
                return ServiceResult<CredentialResponseDto>.Failure("No access token available");
            }

            // Step 2: Create credential request
            var credentialRequest = new CredentialRequestDto
            {
                Type = offerDetails.CredentialType,
                Subject = offerDetails.SubjectId,
                Issuer = offerDetails.IssuerName,
                IssuerId = offerDetails.IssuerId,
                HolderId = "current-user-id", // TODO: Get from user session
                SchemaId = offerDetails.SchemaId,
                Format = CredentialFormat.Jwt,
                IssuanceDate = DateTime.UtcNow,
                ExpirationDate = offerDetails.ExpirationDate,
                Claims = offerDetails.Claims
            };

            // Step 3: Issue credential through wallet service
            var result = await _walletService.CreateCredentialAsync(credentialRequest);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Credential successfully issued and stored: {CredentialId}", result.Data?.Id);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting and storing credential");
            return ServiceResult<CredentialResponseDto>.Failure($"Failed to store credential: {ex.Message}");
        }
    }

    #region Private Helper Methods

    private string BuildCredentialOfferUrl(string credentialType, string issuerId)
    {
        // In a real implementation, this would be received from a QR code or deep link
        // For now, we construct a mock credential offer URL
        var baseUrl = ApiConfiguration.GetIdentityBaseUrl();
        return $"{baseUrl}/credential-offer?type={Uri.EscapeDataString(credentialType)}&issuer={Uri.EscapeDataString(issuerId)}";
    }

    private string BuildAuthorizationUrl(IssuerMetadata metadata, string credentialType)
    {
        var redirectUri = "credvault://oauth-callback";
        var state = Guid.NewGuid().ToString("N");
        var scope = "openid profile credential_issuance";

        return $"{metadata.AuthorizationEndpoint}" +
               $"?response_type=code" +
               $"&client_id={Uri.EscapeDataString(metadata.ClientId)}" +
               $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
               $"&scope={Uri.EscapeDataString(scope)}" +
               $"&state={state}" +
               $"&credential_type={Uri.EscapeDataString(credentialType)}";
    }

    private async Task<ServiceResult<IssuerMetadata>> GetIssuerMetadataAsync(string issuerId)
    {
        try
        {
            // In a real implementation, this would fetch from /.well-known/openid-configuration
            // For now, return mock metadata
            var metadata = new IssuerMetadata
            {
                IssuerId = issuerId,
                AuthorizationEndpoint = $"{ApiConfiguration.GetIdentityBaseUrl()}/connect/authorize",
                TokenEndpoint = $"{ApiConfiguration.GetIdentityBaseUrl()}/connect/token",
                CredentialEndpoint = $"{ApiConfiguration.GetIdentityBaseUrl()}/credential",
                ClientId = "credvault-mobile-app"
            };

            return ServiceResult<IssuerMetadata>.Success(metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting issuer metadata");
            return ServiceResult<IssuerMetadata>.Failure($"Failed to get issuer metadata: {ex.Message}");
        }
    }

    private async Task<ServiceResult<TokenResponse>> ExchangeCodeForTokenAsync(string authorizationCode)
    {
        try
        {
            // In a real implementation, this would call the token endpoint
            // For now, return a mock token response
            var tokenResponse = new TokenResponse
            {
                AccessToken = $"mock_access_token_{Guid.NewGuid():N}",
                RefreshToken = $"mock_refresh_token_{Guid.NewGuid():N}",
                ExpiresIn = 3600,
                TokenType = "Bearer"
            };

            return ServiceResult<TokenResponse>.Success(tokenResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging authorization code for token");
            return ServiceResult<TokenResponse>.Failure($"Failed to exchange code for token: {ex.Message}");
        }
    }

    private async Task StoreTokensAsync(TokenResponse tokenResponse)
    {
        await _secureStorage.SetAsync(AccessTokenKey, tokenResponse.AccessToken);
        await _secureStorage.SetAsync(RefreshTokenKey, tokenResponse.RefreshToken);
        
        var expiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
        await _secureStorage.SetAsync(TokenExpiryKey, expiry.ToString("O"));

        _logger.LogInformation("Tokens stored securely");
    }

    private async Task<ServiceResult<CredentialOfferDetails>> GetCredentialOfferDetailsAsync(
        string credentialType, 
        string issuerId, 
        string accessToken)
    {
        try
        {
            // In a real implementation, this would call the credential endpoint
            // For now, return mock offer details based on credential type
            var offerDetails = credentialType.ToLower() switch
            {
                "nationalid" => new CredentialOfferDetails
                {
                    CredentialType = "NationalID",
                    IssuerName = "Government ID Authority",
                    IssuerId = issuerId,
                    SubjectId = "user-123",
                    SchemaId = "national-id-schema-v1",
                    ExpirationDate = DateTime.UtcNow.AddYears(10),
                    Claims = new Dictionary<string, object>
                    {
                        ["fullName"] = "John Doe",
                        ["dateOfBirth"] = "1990-01-15",
                        ["idNumber"] = "123-456-7890",
                        ["nationality"] = "Citizen",
                        ["photo"] = "base64_photo_data_here"
                    }
                },
                "driverslicense" => new CredentialOfferDetails
                {
                    CredentialType = "DriversLicense",
                    IssuerName = "Department of Motor Vehicles",
                    IssuerId = issuerId,
                    SubjectId = "user-123",
                    SchemaId = "drivers-license-schema-v1",
                    ExpirationDate = DateTime.UtcNow.AddYears(5),
                    Claims = new Dictionary<string, object>
                    {
                        ["fullName"] = "John Doe",
                        ["dateOfBirth"] = "1990-01-15",
                        ["licenseNumber"] = "DL-9876543",
                        ["class"] = "C",
                        ["restrictions"] = "None",
                        ["photo"] = "base64_photo_data_here"
                    }
                },
                "universitydiploma" => new CredentialOfferDetails
                {
                    CredentialType = "UniversityDiploma",
                    IssuerName = "State University",
                    IssuerId = issuerId,
                    SubjectId = "user-123",
                    SchemaId = "university-diploma-schema-v1",
                    ExpirationDate = null,
                    Claims = new Dictionary<string, object>
                    {
                        ["fullName"] = "John Doe",
                        ["degree"] = "Bachelor of Science",
                        ["major"] = "Computer Science",
                        ["graduationDate"] = "2020-05-15",
                        ["gpa"] = "3.8",
                        ["honors"] = "Cum Laude"
                    }
                },
                _ => throw new ArgumentException($"Unknown credential type: {credentialType}")
            };

            return ServiceResult<CredentialOfferDetails>.Success(offerDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting credential offer details");
            return ServiceResult<CredentialOfferDetails>.Failure($"Failed to get credential offer: {ex.Message}");
        }
    }

    #endregion
}

#region Supporting Models

public class IssuerMetadata
{
    public string IssuerId { get; set; } = string.Empty;
    public string AuthorizationEndpoint { get; set; } = string.Empty;
    public string TokenEndpoint { get; set; } = string.Empty;
    public string CredentialEndpoint { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
}

public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = string.Empty;
}

public class CredentialOfferDetails
{
    public string CredentialType { get; set; } = string.Empty;
    public string IssuerName { get; set; } = string.Empty;
    public string IssuerId { get; set; } = string.Empty;
    public string SubjectId { get; set; } = string.Empty;
    public string SchemaId { get; set; } = string.Empty;
    public DateTime? ExpirationDate { get; set; }
    public Dictionary<string, object> Claims { get; set; } = new();
}

#endregion
