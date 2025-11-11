# Credential Issuance Flow - Requirements & Implementation Plan

## 📋 Executive Summary

**Current Status**: UI prototype with mock data (15% complete)
**Target**: Fully functional OpenID4VCI credential issuance flow (100%)
**Priority**: HIGH - Core wallet functionality
**Estimated Effort**: 3-5 days

---

## 🎯 Business Requirements

### Primary Objectives
1. Enable users to request and receive verifiable credentials from trusted issuers
2. Implement industry-standard OpenID4VCI protocol for credential issuance
3. Provide secure OAuth 2.0 authentication with issuers
4. Store credentials securely in the mobile wallet
5. Display credentials on dashboard with status indicators

### User Story
```
As a wallet holder
I want to add new credentials from government and institutional issuers
So that I can store and present my verifiable credentials digitally
```

### Acceptance Criteria
- ✅ User can browse available credential types (National ID, Driver's License, Diploma)
- ✅ User can initiate credential issuance flow with selected issuer
- ✅ System launches browser for OAuth authentication
- ✅ System handles OAuth callback and exchanges authorization code for tokens
- ✅ System requests credential offer from issuer
- ✅ User reviews credential claims before accepting
- ✅ System stores issued credential in wallet
- ✅ Credential appears on dashboard
- ✅ User can view credential details
- ✅ All operations have proper error handling

---

## 🏗️ Architecture Overview

### Flow Diagram
```
┌─────────────────────────────────────────────────────────────────┐
│ 1. SELECT CREDENTIAL TYPE                                        │
│    User: Tap "National ID" → Tap "Continue"                     │
└──────────────────────────┬──────────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────────┐
│ 2. START AUTHENTICATION FLOW                                     │
│    App → AuthenticationFlowService.StartCredentialIssuanceFlow() │
│    Response: Authorization URL                                   │
└──────────────────────────┬──────────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────────┐
│ 3. OAUTH BROWSER AUTHENTICATION                                  │
│    App → WebAuthenticator.AuthenticateAsync()                   │
│    Browser opens: https://identity.gov/oauth/authorize?...      │
│    User logs in with issuer (Gov ID, University, etc.)          │
│    Browser redirects: credvault://oauth-callback?code=...       │
└──────────────────────────┬──────────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────────┐
│ 4. EXCHANGE AUTHORIZATION CODE                                   │
│    App → AuthenticationFlowService.HandleOAuthCallback()        │
│    POST /api/v1/wallet/Authorization/token                      │
│    Response: { accessToken, refreshToken, cNonce, ... }         │
└──────────────────────────┬──────────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────────┐
│ 5. REQUEST CREDENTIAL OFFER                                      │
│    App → AuthenticationFlowService.RequestCredentialIssuance()  │
│    GET /api/v1/wallet/CredentialDiscovery/credential_offer     │
│    Response: { claims, issuer, schema, expiry, ... }            │
└──────────────────────────┬──────────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────────┐
│ 6. REVIEW & CONSENT                                              │
│    User: Reviews claims (Name, DOB, Photo, ID#, etc.)          │
│    User: ☑ I have reviewed  ☑ I consent                        │
│    User: Tap "Confirm Issuance"                                 │
└──────────────────────────┬──────────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────────┐
│ 7. ISSUE & STORE CREDENTIAL                                      │
│    App → AuthenticationFlowService.AcceptAndStoreCredential()   │
│    POST /api/v1/wallet/credential                               │
│    Request: { format, proof, credentialSubject, ... }           │
│    Response: { credential, transactionId, ... }                 │
│    App → SecureStorage: Store credential                         │
└──────────────────────────┬──────────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────────┐
│ 8. CONFIRMATION & DASHBOARD UPDATE                               │
│    Show success message                                          │
│    Navigate to Dashboard                                         │
│    Dashboard refreshes → Shows new credential                    │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📦 Technical Components

### 1. API Endpoints Required

#### Authorization Endpoints
| Endpoint | Method | Purpose | Status |
|----------|--------|---------|--------|
| `/api/v1/wallet/Authorization/authorize` | POST | Initiate OAuth flow | ✅ Interface defined |
| `/api/v1/wallet/Authorization/token` | POST | Exchange auth code for token | ✅ Interface defined |
| `/api/v1/wallet/Authorization/validate` | POST | Validate access token | ✅ Interface defined |

#### Credential Discovery
| Endpoint | Method | Purpose | Status |
|----------|--------|---------|--------|
| `/api/v1/wallet/CredentialDiscovery/credential_offer` | GET | Get credential offer details | ✅ Interface defined |

#### Credential Issuance
| Endpoint | Method | Purpose | Status |
|----------|--------|---------|--------|
| `/api/v1/wallet/credential` | POST | Issue credential to holder | ✅ Interface defined |
| `/api/v1/wallet/batch_credential` | POST | Issue multiple credentials | ✅ Interface defined |

#### Wallet/Holder Operations
| Endpoint | Method | Purpose | Status |
|----------|--------|---------|--------|
| `/api/v1/wallet/Credential/holder/{holderId}` | GET | Get holder's credentials | ✅ Interface defined |
| `/api/v1/wallet/Holder` | POST | Create holder if not exists | ✅ Interface defined |

---

### 2. Data Models

#### ✅ Already Defined
- `OpenID4VCICredentialRequestDto`
- `OpenID4VCICredentialResponseDto`
- `AuthorizationRequestDto`
- `AuthorizationResponseDto`
- `TokenRequestDto`
- `TokenResponseDto`
- `CredentialResponseDto`
- `CredentialRequestDto`

#### ⚠️ Needs Enhancement
```csharp
// Add to Models/OpenID4VCI.cs

/// <summary>
/// Credential offer details from issuer
/// </summary>
public class CredentialOfferDetails
{
    public required string CredentialType { get; set; }
    public required string IssuerId { get; set; }
    public required string IssuerName { get; set; }
    public string? IssuerLogoUrl { get; set; }
    public required Dictionary<string, object> Claims { get; set; }
    public string? SchemaId { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? Format { get; set; }
    public string? CredentialIdentifier { get; set; }
    public string? CNonce { get; set; }
}

/// <summary>
/// Issuer metadata for OpenID4VCI
/// </summary>
public class IssuerMetadata
{
    public required string IssuerId { get; set; }
    public required string IssuerName { get; set; }
    public required string AuthorizationEndpoint { get; set; }
    public required string TokenEndpoint { get; set; }
    public required string CredentialEndpoint { get; set; }
    public List<string>? SupportedCredentialTypes { get; set; }
    public Dictionary<string, object>? AdditionalMetadata { get; set; }
}
```

---

### 3. Services Architecture

#### AuthenticationFlowService (✅ Partially Implemented)

**Current Status**: 
- Service class exists with proper DI
- Has placeholder methods for OAuth flow
- Needs complete implementation

**Required Methods**:
```csharp
public class AuthenticationFlowService
{
    // ✅ Already defined
    Task<ServiceResult<string>> StartCredentialIssuanceFlowAsync(string credentialType, string issuerId);
    Task<ServiceResult<string>> HandleOAuthCallbackAsync(string authorizationCode, string state);
    
    // ⚠️ Needs implementation
    Task<ServiceResult<CredentialOfferDetails>> RequestCredentialIssuanceAsync(string credentialType, string issuerId);
    Task<ServiceResult<CredentialResponseDto>> AcceptAndStoreCredentialAsync(CredentialOfferDetails offer, bool userConsented);
    Task<ServiceResult<IssuerMetadata>> GetIssuerMetadataAsync(string issuerId);
    Task<ServiceResult<TokenResponseDto>> ExchangeCodeForTokenAsync(string authorizationCode);
    Task<ServiceResult<bool>> StoreTokensAsync(TokenResponseDto tokens);
    Task<ServiceResult<string?>> GetAccessTokenAsync();
}
```

#### WalletService (✅ Implemented)
- Already has all required credential CRUD operations
- `GetCredentialsAsync()` ✅
- `GetCredentialByIdAsync()` ✅
- `CreateCredentialAsync()` ✅
- No changes needed

---

### 4. ViewModels

#### AddCredentialViewModel (⚠️ Needs Updates)

**Current State**: Uses mock authentication with fake progress bar

**Required Changes**:
```csharp
// REPLACE: StartMockAuthenticationFlowAsync()
// WITH: StartRealAuthenticationFlowAsync()

private async Task StartRealAuthenticationFlowAsync()
{
    // 1. Get authorization URL
    var authUrlResult = await _authFlowService.StartCredentialIssuanceFlowAsync(
        SelectedCredentialType.Type,
        SelectedCredentialType.IssuerId
    );

    if (!authUrlResult.IsSuccess)
    {
        await ShowAlertAsync("Error", authUrlResult.ErrorMessage, "OK");
        return;
    }

    // 2. Launch browser for OAuth
#if ANDROID || IOS
    var authResult = await WebAuthenticator.Default.AuthenticateAsync(
        new WebAuthenticatorOptions
        {
            Url = new Uri(authUrlResult.Data!),
            CallbackUrl = new Uri("credvault://oauth-callback"),
            PrefersEphemeralWebBrowserSession = true
        });
#else
    // Windows: Open browser and listen for callback
    throw new NotImplementedException("Windows OAuth not yet implemented");
#endif

    // 3. Handle callback
    var code = authResult.Properties["code"];
    var state = authResult.Properties["state"];
    
    var tokenResult = await _authFlowService.HandleOAuthCallbackAsync(code, state);
    
    if (!tokenResult.IsSuccess)
    {
        await ShowAlertAsync("Authentication Failed", tokenResult.ErrorMessage, "OK");
        return;
    }

    // 4. Request credential offer
    var offerResult = await _authFlowService.RequestCredentialIssuanceAsync(
        SelectedCredentialType.Type,
        SelectedCredentialType.IssuerId
    );

    if (!offerResult.IsSuccess)
    {
        await ShowAlertAsync("Error", offerResult.ErrorMessage, "OK");
        return;
    }

    // 5. Load real claims and show consent page
    _credentialOffer = offerResult.Data;
    await LoadCredentialClaimsAsync(offerResult.Data!);
    await ProceedToConsentReviewAsync();
}

// UPDATE: ConfirmIssuanceAsync
[RelayCommand(CanExecute = nameof(CanConfirmIssuance))]
private async Task ConfirmIssuanceAsync()
{
    if (!HasReviewedClaims || !HasConsentedToIssuance || _credentialOffer == null)
        return;

    try
    {
        IsBusy = true;

        // REAL credential issuance
        var result = await _authFlowService.AcceptAndStoreCredentialAsync(
            _credentialOffer,
            userConsented: true
        );

        if (result.IsSuccess)
        {
            NewCredentialId = result.Data?.Id;
            CredentialAddedSuccessfully = true;
            CurrentStep = 4;
            
            // Navigate to confirmation
            await Task.Delay(500); // Brief delay to show UI
            await _navigationService.NavigateToAsync("confirmation");
        }
        else
        {
            await ShowAlertAsync("Error", result.ErrorMessage ?? "Failed to issue credential", "OK");
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to confirm credential issuance");
        await ShowAlertAsync("Error", "Failed to add credential", "OK");
    }
    finally
    {
        IsBusy = false;
    }
}
```

#### DashboardViewModel (⚠️ Needs Enhancement)

**Current State**: Shows "No credentials yet" correctly, but needs real credential loading

**Required Changes**:
```csharp
[RelayCommand]
private async Task RefreshCredentialsAsync()
{
    try
    {
        IsBusy = true;
        
        // Get holder ID from secure storage
        var holderIdString = await SecureStorage.GetAsync("holder_id");
        
        if (string.IsNullOrEmpty(holderIdString))
        {
            // First time - create holder
            await CreateHolderAsync();
            return;
        }

        // Load credentials for this holder
        var holderId = Guid.Parse(holderIdString);
        var result = await _walletService.GetHolderCredentialsAsync(holderId);
        
        Credentials.Clear();
        
        if (result.IsSuccess && result.Data != null && result.Data.Any())
        {
            foreach (var cred in result.Data.Take(5))
            {
                Credentials.Add(new CredentialCardInfo
                {
                    Id = cred.Id ?? string.Empty,
                    Name = cred.Type ?? "Unknown Credential",
                    Icon = GetIconForType(cred.Type ?? ""),
                    ExpiryInfo = GetExpiryText(cred.ExpirationDate),
                    StatusColor = GetStatusColor(cred.Status.ToString())
                });
            }
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error refreshing credentials");
    }
    finally
    {
        IsBusy = false;
    }
}

private async Task CreateHolderAsync()
{
    try
    {
        var userId = await SecureStorage.GetAsync("user_id");
        var email = await SecureStorage.GetAsync("email");
        
        if (string.IsNullOrEmpty(userId))
            return;

        var holderRequest = new HolderRegistrationDto
        {
            UserId = Guid.Parse(userId),
            Email = email ?? "",
            FirstName = await SecureStorage.GetAsync("first_name") ?? "",
            LastName = await SecureStorage.GetAsync("last_name") ?? ""
        };

        var result = await _walletService.CreateHolderAsync(holderRequest);
        
        if (result.IsSuccess && result.Data != null)
        {
            await SecureStorage.SetAsync("holder_id", result.Data.Id.ToString());
            _logger.LogInformation("Holder created: {HolderId}", result.Data.Id);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating holder");
    }
}
```

---

### 5. Platform-Specific Configuration

#### Android (AndroidManifest.xml)
```xml
<activity android:name="crc64e1fb321c08285b90.MainActivity" 
          android:exported="true"
          android:launchMode="singleTask">
    
    <!-- Existing launcher intent -->
    <intent-filter>
        <action android:name="android.intent.action.MAIN" />
        <category android:name="android.intent.category.LAUNCHER" />
    </intent-filter>
    
    <!-- OAuth callback intent filter -->
    <intent-filter>
        <action android:name="android.intent.action.VIEW" />
        <category android:name="android.intent.category.DEFAULT" />
        <category android:name="android.intent.category.BROWSABLE" />
        <data android:scheme="credvault" 
              android:host="oauth-callback" />
    </intent-filter>
</activity>
```

#### iOS (Info.plist)
```xml
<key>CFBundleURLTypes</key>
<array>
    <dict>
        <key>CFBundleURLName</key>
        <string>CredVault OAuth</string>
        <key>CFBundleURLSchemes</key>
        <array>
            <string>credvault</string>
        </array>
    </dict>
</array>

<key>LSApplicationQueriesSchemes</key>
<array>
    <string>https</string>
    <string>http</string>
</array>
```

#### Windows (Package.appxmanifest)
```xml
<Extensions>
    <uap:Extension Category="windows.protocol">
        <uap:Protocol Name="credvault">
            <uap:DisplayName>CredVault</uap:DisplayName>
        </uap:Protocol>
    </uap:Extension>
</Extensions>
```

---

## 🔒 Security Requirements

### 1. OAuth 2.0 Security
- ✅ Use PKCE (Proof Key for Code Exchange) for authorization
- ✅ Generate secure random state parameter for CSRF protection
- ✅ Validate state parameter on callback
- ✅ Use ephemeral browser sessions (no shared cookies)
- ✅ Short-lived authorization codes (5 minutes)

### 2. Token Security
- ✅ Store access tokens in SecureStorage (encrypted)
- ✅ Store refresh tokens in SecureStorage (encrypted)
- ✅ Implement token rotation on refresh
- ✅ Clear tokens on logout
- ✅ Validate token expiry before each API call

### 3. Credential Security
- ✅ Store credentials in SecureStorage (encrypted at rest)
- ✅ Verify credential signatures from issuer
- ✅ Validate credential schema and structure
- ✅ Check credential expiration dates
- ✅ Implement credential revocation checks

### 4. Communication Security
- ✅ All API calls over HTTPS
- ✅ Certificate pinning for production APIs
- ✅ Validate SSL certificates
- ✅ Use Ocp-Apim-Subscription-Key header for Azure APIM

---

## 🧪 Testing Requirements

### Unit Tests
```csharp
// Tests to create:
AuthenticationFlowServiceTests
- StartCredentialIssuanceFlowAsync_ValidType_ReturnsAuthUrl
- HandleOAuthCallbackAsync_ValidCode_ReturnsAccessToken
- RequestCredentialIssuanceAsync_ValidRequest_ReturnsOffer
- AcceptAndStoreCredentialAsync_ValidOffer_StoresCredential

AddCredentialViewModelTests
- ProceedToAuthentication_ValidType_LaunchesWebAuthenticator
- ConfirmIssuance_ValidConsent_IssuesCredential
- ConfirmIssuance_InvalidConsent_ShowsError

WalletServiceTests (already exist)
- GetHolderCredentialsAsync_ValidHolder_ReturnsCredentials
- CreateCredentialAsync_ValidRequest_ReturnsCreated
```

### Integration Tests
```csharp
CredentialIssuanceFlowTests
- EndToEnd_CompleteFlow_IssuesCredential
- OAuth_UserCancels_HandlesGracefully
- OAuth_InvalidCode_ReturnsError
- CredentialOffer_Expired_HandlesGracefully
```

### Manual Test Scenarios
1. **Happy Path**: Select credential → Authenticate → Review → Accept → See on dashboard
2. **User Cancels**: Start flow → Cancel browser → Returns to select screen
3. **Network Error**: Start flow → Disconnect internet → Shows error
4. **Invalid Credential Type**: Select unknown type → Shows error
5. **Expired Offer**: Get offer → Wait → Accept → Shows expired error
6. **Duplicate Credential**: Issue same credential twice → Handles gracefully

---

## 📊 API Response Format Expectations

### Important: Based on Authentication Learning
From our authentication debugging, we learned that Azure APIs **return direct responses**, not wrapped in `ApiResponseDto`.

#### Current Pattern (Confirmed Working)
```csharp
// Login API returns direct response
public Task<LoginResponseDto> LoginAsync([Body] LoginRequestDto request);

// Response: { accessToken, refreshToken, user: {...} }
```

#### Expected Pattern for Credential APIs
We need to **test actual API responses** before finalizing, but anticipate:

**Option 1: Direct Response (Likely)**
```csharp
[Post("/api/v1/wallet/Authorization/token")]
Task<TokenResponseDto> ExchangeAuthorizationCodeAsync([Body] TokenRequestDto request);
```

**Option 2: Wrapped Response (Less Likely)**
```csharp
[Post("/api/v1/wallet/Authorization/token")]
Task<ApiResponseDto<TokenResponseDto>> ExchangeAuthorizationCodeAsync([Body] TokenRequestDto request);
```

**Action Required**: Test with curl/Postman first, then adjust interfaces.

---

## 🚀 Implementation Phases

### Phase 1: Foundation (Day 1) ✅ PARTIALLY COMPLETE
- [x] Create API client interfaces (IWalletApiClient) ✅
- [x] Define data models (OpenID4VCI DTOs) ✅
- [x] Create AuthenticationFlowService skeleton ✅
- [ ] Add platform-specific deep link configuration
- [ ] Register services in DI container
- [ ] Add CredentialOfferDetails model
- [ ] Add IssuerMetadata model

### Phase 2: OAuth Implementation (Day 2)
- [ ] Implement `StartCredentialIssuanceFlowAsync()`
  - Build authorization URL with PKCE
  - Generate state parameter
  - Store state for validation
- [ ] Implement `HandleOAuthCallbackAsync()`
  - Validate state parameter
  - Extract authorization code
  - Call ExchangeCodeForTokenAsync()
  - Store tokens in SecureStorage
- [ ] Test OAuth flow with mock issuer
- [ ] Add error handling and logging

### Phase 3: Credential Issuance (Day 3)
- [ ] Implement `RequestCredentialIssuanceAsync()`
  - Call GET /api/v1/wallet/CredentialDiscovery/credential_offer
  - Parse credential offer
  - Map to CredentialOfferDetails
- [ ] Implement `AcceptAndStoreCredentialAsync()`
  - Build OpenID4VCICredentialRequestDto
  - Call POST /api/v1/wallet/credential
  - Parse response
  - Store in SecureStorage
  - Create holder record if needed
- [ ] Test credential issuance with mock data
- [ ] Verify credential storage

### Phase 4: ViewModel Integration (Day 4)
- [ ] Update AddCredentialViewModel
  - Replace StartMockAuthenticationFlowAsync()
  - Integrate WebAuthenticator
  - Handle OAuth callback
  - Load real credential claims
  - Update UI progress indicators
- [ ] Update DashboardViewModel
  - Implement RefreshCredentialsAsync()
  - Create holder on first launch
  - Load holder credentials
  - Update UI with real data
- [ ] Update CredentialDetailsViewModel (already mostly complete)
- [ ] Test end-to-end flow in emulator

### Phase 5: Error Handling & Polish (Day 5)
- [ ] Add comprehensive error messages
- [ ] Implement retry logic for network failures
- [ ] Add loading indicators
- [ ] Add success animations
- [ ] Handle edge cases:
  - User cancels OAuth
  - Network timeout
  - Expired credentials
  - Duplicate credentials
  - Invalid issuer
- [ ] Add analytics logging
- [ ] Performance optimization

### Phase 6: Testing & Documentation (Day 6)
- [ ] Write unit tests for AuthenticationFlowService
- [ ] Write unit tests for ViewModels
- [ ] Integration tests for complete flow
- [ ] Manual testing on Android emulator
- [ ] Manual testing on physical device
- [ ] Update documentation
- [ ] Create developer guide
- [ ] Record demo video

---

## 📝 API Testing Plan

### Before Implementation: Test Actual Endpoints

```powershell
# 1. Test Authorization Initiation
$headers = @{
    "Ocp-Apim-Subscription-Key" = "4a47f13f76d54eb999efc2036245ddc2"
    "Content-Type" = "application/json"
}

$authRequest = @{
    credentialType = "NationalID"
    redirectUri = "credvault://oauth-callback"
    state = "random-state-123"
    scope = "openid credential"
    codeChallenge = "test-code-challenge"
    codeChallengeMethod = "S256"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://apim-wallet-dev.azure-api.net/wallet/api/v1/Authorization/authorize" `
    -Method POST `
    -Headers $headers `
    -Body $authRequest

# 2. Test Token Exchange
$tokenRequest = @{
    grantType = "authorization_code"
    code = "AUTH_CODE_HERE"
    redirectUri = "credvault://oauth-callback"
    codeVerifier = "test-code-verifier"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://apim-wallet-dev.azure-api.net/wallet/api/v1/Authorization/token" `
    -Method POST `
    -Headers $headers `
    -Body $tokenRequest

# 3. Test Credential Offer
Invoke-RestMethod -Uri "https://apim-wallet-dev.azure-api.net/wallet/api/v1/CredentialDiscovery/credential_offer?credential_type=NationalID&issuer_id=gov-id-authority" `
    -Method GET `
    -Headers $headers

# 4. Test Credential Issuance
$credRequest = @{
    format = "jwt_vc_json"
    credentialIdentifier = "NationalID"
    credentialSubject = @{
        firstName = "John"
        lastName = "Doe"
        dateOfBirth = "1990-01-01"
    }
    proof = @{
        proofType = "jwt"
        jwt = "JWT_TOKEN_HERE"
    }
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://apim-wallet-dev.azure-api.net/wallet/api/v1/credential" `
    -Method POST `
    -Headers $headers `
    -Body $credRequest
```

**Document Response Structures**: Save actual JSON responses to update DTOs if needed.

---

## 🎯 Success Metrics

### Functional Metrics
- ✅ User can complete credential issuance flow without errors
- ✅ Credentials appear on dashboard within 2 seconds
- ✅ OAuth authentication completes in < 30 seconds
- ✅ Error messages are clear and actionable
- ✅ No crashes or exceptions during flow

### Performance Metrics
- OAuth flow: < 30 seconds
- Credential issuance: < 5 seconds
- Dashboard refresh: < 2 seconds
- API response time: < 1 second per call

### Security Metrics
- ✅ All tokens stored in SecureStorage
- ✅ No sensitive data in logs
- ✅ All API calls over HTTPS
- ✅ OAuth state validation 100% success rate

---

## 🐛 Known Issues & Risks

### Technical Risks
1. **Mock Issuer Not Available**: May need to create mock issuer service for testing
2. **WebAuthenticator Platform Differences**: iOS/Android/Windows behavior varies
3. **Deep Link Handling**: Complex to test without physical device
4. **Token Expiry**: Need to handle token refresh gracefully
5. **Network Failures**: Need robust retry logic

### Mitigation Strategies
1. Create mock OAuth endpoints in test environment
2. Test on all platforms early
3. Use emulator with ADB for deep link testing
4. Implement automatic token refresh
5. Add offline mode with queue

---

## 📚 Dependencies

### NuGet Packages (Already Installed)
- ✅ Microsoft.Maui.Authentication (WebAuthenticator)
- ✅ Refit (HTTP client)
- ✅ CommunityToolkit.Mvvm (ViewModels)
- ✅ Microsoft.Extensions.Logging

### External Services
- ✅ Azure APIM (apim-wallet-dev.azure-api.net)
- ✅ Identity Service (deployed)
- ✅ Wallet Service (deployed)
- ⚠️ Issuer Services (may need mock implementation)

---

## 🎓 Reference Materials

### OpenID4VCI Specification
- [OpenID for Verifiable Credential Issuance](https://openid.net/specs/openid-4-verifiable-credential-issuance-1_0.html)
- [OAuth 2.0 Authorization Framework](https://datatracker.ietf.org/doc/html/rfc6749)
- [PKCE RFC](https://datatracker.ietf.org/doc/html/rfc7636)

### MAUI Documentation
- [WebAuthenticator](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/communication/authentication)
- [Deep Linking](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/communication/deep-linking)
- [Secure Storage](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/secure-storage)

---

## ✅ Next Steps

### Immediate Actions (Today)
1. ✅ Complete requirements assessment (DONE)
2. ⏭️ Test Azure Wallet API endpoints with curl/Postman
3. ⏭️ Document actual API response formats
4. ⏭️ Update IWalletApiClient if needed based on testing
5. ⏭️ Add platform-specific deep link configuration

### This Week
1. Implement Phase 2: OAuth Implementation
2. Implement Phase 3: Credential Issuance
3. Implement Phase 4: ViewModel Integration
4. Test end-to-end flow

### Next Week
1. Phase 5: Error handling and polish
2. Phase 6: Testing and documentation
3. Deploy to test environment
4. User acceptance testing

---

## 📞 Questions to Resolve

1. **Issuer Endpoints**: Are real issuer services deployed, or do we need mocks?
2. **Holder Creation**: Should holders be created on registration or first credential add?
3. **Credential Storage**: Store in database or SecureStorage only?
4. **Offline Support**: Should credentials be accessible offline?
5. **Credential Refresh**: How to handle expired credentials?

---

**Document Version**: 1.0  
**Last Updated**: January 10, 2025  
**Author**: GitHub Copilot  
**Status**: Ready for Implementation
