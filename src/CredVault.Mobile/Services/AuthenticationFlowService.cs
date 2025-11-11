using CredVault.Mobile.Models;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http.Json;

namespace CredVault.Mobile.Services;

/// <summary>
/// Service for handling OAuth 2.0 and OpenID4VCI authentication flows with PKCE
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
    private const string PKCEVerifierKey = "pkce_code_verifier";
    private const string PKCEStateKey = "pkce_state";

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
    /// Start the credential issuance flow with OpenID4VCI and PKCE
    /// Opens browser for authentication and returns when complete
    /// </summary>
    public async Task<ServiceResult<string>> StartCredentialIssuanceFlowAsync(string credentialType, string issuerId)
    {
        try
        {
            _logger.LogInformation("Starting credential issuance flow for type: {CredentialType}, issuer: {IssuerId}", 
                credentialType, issuerId);

            // Step 1: Generate PKCE parameters
            var pkce = GeneratePKCEParameters();
            await _secureStorage.SetAsync(PKCEVerifierKey, pkce.CodeVerifier);
            await _secureStorage.SetAsync(PKCEStateKey, pkce.State);

            // Step 2: Build authorization URL with PKCE challenge
            var authUrl = BuildAuthorizationUrlWithPKCE(credentialType, issuerId, pkce);

            _logger.LogInformation("Opening browser for authentication with PKCE");

            // Step 3: Open browser and wait for callback
            var result = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri(authUrl),
                new Uri("credvault://oauth-callback"));

            // Step 4: Extract authorization code from callback
            if (result.Properties.TryGetValue("code", out var authCode) &&
                result.Properties.TryGetValue("state", out var returnedState))
            {
                // Validate state to prevent CSRF attacks
                var storedState = await _secureStorage.GetAsync(PKCEStateKey);
                if (storedState != returnedState)
                {
                    return ServiceResult<string>.Failure("State mismatch - possible CSRF attack");
                }

                // Step 5: Exchange authorization code for access token using PKCE verifier
                var tokenResult = await ExchangeCodeForTokenWithPKCEAsync(authCode);
                if (!tokenResult.IsSuccess)
                {
                    return ServiceResult<string>.Failure("Failed to exchange authorization code for token");
                }

                return ServiceResult<string>.Success(tokenResult.Data?.AccessToken ?? string.Empty);
            }

            return ServiceResult<string>.Failure("No authorization code received");
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("User cancelled authentication");
            return ServiceResult<string>.Failure("Authentication cancelled by user");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting credential issuance flow");
            return ServiceResult<string>.Failure($"Failed to start credential issuance: {ex.Message}");
        }
        finally
        {
            // Clean up PKCE parameters
            _secureStorage.Remove(PKCEVerifierKey);
            _secureStorage.Remove(PKCEStateKey);
        }
    }

    /// <summary>
    /// Request credential offer from wallet API after authentication
    /// </summary>
    public async Task<ServiceResult<CredentialOfferDetails>> RequestCredentialIssuanceAsync(
        string credentialType, 
        string issuerId)
    {
        try
        {
            _logger.LogInformation("Requesting credential offer from wallet for type: {CredentialType}", credentialType);

            // Step 1: Get stored access token
            var accessToken = await _secureStorage.GetAsync(AccessTokenKey);
            if (string.IsNullOrEmpty(accessToken))
            {
                return ServiceResult<CredentialOfferDetails>.Failure("No access token available. Please authenticate first.");
            }

            // Step 2: Call wallet API to get credential offer
            // This will call: GET /api/v1/wallet/discovery/credential_offer
            var walletBaseUrl = ApiConfiguration.GetWalletBaseUrl();
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ApiConfiguration.Azure.SubscriptionKey);

            var url = $"{walletBaseUrl}/api/v1/wallet/discovery/credential_offer?type={Uri.EscapeDataString(credentialType)}&issuerId={Uri.EscapeDataString(issuerId)}";
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Credential offer request failed: {StatusCode} - {Error}", response.StatusCode, error);
                return ServiceResult<CredentialOfferDetails>.Failure($"Failed to get credential offer: {response.StatusCode}");
            }

            // TODO: Parse actual response from API based on Swagger schema
            // For now, return mock data
            var mockOffer = new CredentialOfferDetails
            {
                CredentialType = credentialType,
                IssuerName = "Test Issuer",
                IssuerId = issuerId,
                Claims = new Dictionary<string, object>
                {
                    ["fullName"] = "John Doe",
                    ["email"] = "john.doe@example.com"
                }
            };

            return ServiceResult<CredentialOfferDetails>.Success(mockOffer);
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
                Subject = "current-user", // Subject from authenticated user
                Issuer = offerDetails.IssuerName,
                IssuerId = offerDetails.IssuerId,
                HolderId = "current-user-id", // TODO: Get from user session
                SchemaId = offerDetails.SchemaId ?? string.Empty,
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

    /// <summary>
    /// Handle OAuth callback - exchanges authorization code for access token
    /// This is a public wrapper for use by ViewModels
    /// </summary>
    public async Task<ServiceResult<string>> HandleOAuthCallbackAsync(string authorizationCode, string state)
    {
        try
        {
            _logger.LogInformation("Handling OAuth callback");

            // Validate state to prevent CSRF attacks
            var storedState = await _secureStorage.GetAsync(PKCEStateKey);
            if (storedState != state)
            {
                return ServiceResult<string>.Failure("State mismatch - possible CSRF attack");
            }

            // Exchange authorization code for access token using PKCE verifier
            var tokenResult = await ExchangeCodeForTokenWithPKCEAsync(authorizationCode);
            if (!tokenResult.IsSuccess)
            {
                return ServiceResult<string>.Failure(tokenResult.ErrorMessage ?? "Failed to exchange authorization code for token");
            }

            return ServiceResult<string>.Success(tokenResult.Data?.AccessToken ?? string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling OAuth callback");
            return ServiceResult<string>.Failure($"Failed to handle OAuth callback: {ex.Message}");
        }
        finally
        {
            // Clean up PKCE parameters
            _secureStorage.Remove(PKCEVerifierKey);
            _secureStorage.Remove(PKCEStateKey);
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Generate PKCE parameters for secure OAuth flow
    /// </summary>
    private PKCEParameters GeneratePKCEParameters()
    {
        // Generate cryptographically random code verifier (43-128 characters)
        var codeVerifierBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(codeVerifierBytes);
        }
        var codeVerifier = Convert.ToBase64String(codeVerifierBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        // Generate code challenge using SHA256
        byte[] challengeBytes;
        using (var sha256 = SHA256.Create())
        {
            challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
        }
        var codeChallenge = Convert.ToBase64String(challengeBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        return new PKCEParameters
        {
            CodeVerifier = codeVerifier,
            CodeChallenge = codeChallenge,
            CodeChallengeMethod = "S256",
            State = Guid.NewGuid().ToString("N")
        };
    }

    /// <summary>
    /// Build authorization URL with PKCE challenge
    /// </summary>
    private string BuildAuthorizationUrlWithPKCE(string credentialType, string issuerId, PKCEParameters pkce)
    {
        var authUrl = $"{ApiConfiguration.Azure.OAuth.AuthorizationEndpoint}" +
                      $"?response_type=code" +
                      $"&client_id={ApiConfiguration.Azure.OAuth.ClientId}" +
                      $"&redirect_uri={Uri.EscapeDataString(ApiConfiguration.Azure.OAuth.RedirectUri)}" +
                      $"&scope={Uri.EscapeDataString(string.Join(" ", ApiConfiguration.Azure.OAuth.Scopes))}" +
                      $"&state={pkce.State}" +
                      $"&code_challenge={pkce.CodeChallenge}" +
                      $"&code_challenge_method={pkce.CodeChallengeMethod}" +
                      $"&credential_type={Uri.EscapeDataString(credentialType)}" +
                      $"&issuer_id={Uri.EscapeDataString(issuerId)}";

        return authUrl;
    }

    /// <summary>
    /// Exchange authorization code for access token using PKCE verifier
    /// </summary>
    private async Task<ServiceResult<TokenResponse>> ExchangeCodeForTokenWithPKCEAsync(string authorizationCode)
    {
        try
        {
            var codeVerifier = await _secureStorage.GetAsync(PKCEVerifierKey);
            if (string.IsNullOrEmpty(codeVerifier))
            {
                return ServiceResult<TokenResponse>.Failure("PKCE code verifier not found");
            }

            using var httpClient = new HttpClient();
            var tokenRequestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", authorizationCode),
                new KeyValuePair<string, string>("redirect_uri", ApiConfiguration.Azure.OAuth.RedirectUri),
                new KeyValuePair<string, string>("client_id", ApiConfiguration.Azure.OAuth.ClientId),
                new KeyValuePair<string, string>("code_verifier", codeVerifier)
            });

            var response = await httpClient.PostAsync(ApiConfiguration.Azure.OAuth.TokenEndpoint, tokenRequestContent);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Token exchange failed: {StatusCode} - {Error}", response.StatusCode, error);
                return ServiceResult<TokenResponse>.Failure($"Token exchange failed: {response.StatusCode}");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
            if (tokenResponse == null)
            {
                return ServiceResult<TokenResponse>.Failure("Invalid token response");
            }

            // Store tokens securely
            await StoreTokensAsync(tokenResponse);

            return ServiceResult<TokenResponse>.Success(tokenResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging authorization code for token with PKCE");
            return ServiceResult<TokenResponse>.Failure($"Failed to exchange code for token: {ex.Message}");
        }
    }

    private async Task StoreTokensAsync(TokenResponse tokenResponse)
    {
        await _secureStorage.SetAsync(AccessTokenKey, tokenResponse.AccessToken);
        
        if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
        {
            await _secureStorage.SetAsync(RefreshTokenKey, tokenResponse.RefreshToken);
        }
        
        var expiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
        await _secureStorage.SetAsync(TokenExpiryKey, expiry.ToString("O"));

        _logger.LogInformation("Tokens stored securely");
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

#endregion
