# CredVault ×  GovStack Backend Integration Report

**Date:** November 2, 2025  
**Status:** 🟡 Partially Integrated  
**Backend Services:** Running on `http://localhost:8000`

---

## 🎯 Executive Summary

The CredVault mobile app has basic authentication infrastructure but requires updates to align with the actual GovStack Identity and Wallet API endpoints discovered via Swagger documentation.

**Key Findings:**
- ✅ WebAuthenticator properly configured for OAuth2 flows
- ✅ SecureStorage implementation for token management
- ❌ **CRITICAL:** No Bearer token injection in HTTP clients
- ❌ API client interfaces don't match actual backend endpoints
- ❌ Missing user registration implementation

---

## 🔐 Identity Service API (`/api/v1/identity`)

### Available Endpoints

#### Authentication & Authorization
- ✅ `POST /api/v1/identity/auth/register` - User registration
- ✅ `POST /api/v1/identity/auth/login` - User login
- ✅ `POST /api/v1/identity/auth/logout` - User logout
- ✅ `POST /api/v1/identity/auth/refresh` - Refresh access token
- ✅ `POST /api/v1/identity/auth/validate` - Validate token
- ✅ `GET /api/v1/identity/auth/userinfo` - Get user profile

#### OAuth2/OIDC Endpoints
- ✅ `GET /connect/authorize` - Authorization endpoint
- ✅ `POST /connect/token` - Token endpoint
- ✅ `GET /connect/userinfo` - OIDC UserInfo
- ✅ `POST /connect/revocation` - Token revocation
- ✅ `GET /.well-known/openid-configuration` - OIDC discovery

#### User Profile Management
- ✅ `GET /api/v1/identity/users/profile` - Get current user profile
- ✅ `PUT /api/v1/identity/users/profile` - Update profile
- ✅ `POST /api/v1/identity/users/change-password` - Change password

#### Multi-Factor Authentication
- ✅ `GET /api/v1/identity/mfa/status` - MFA status
- ✅ `POST /api/v1/identity/mfa/totp/setup` - Setup TOTP
- ✅ `POST /api/v1/identity/mfa/totp/enable` - Enable TOTP
- ✅ `POST /api/v1/identity/mfa/totp/disable` - Disable TOTP
- ✅ `POST /api/v1/identity/mfa/verify` - Verify MFA code
- ✅ `POST /api/v1/identity/mfa/webauthn/register/begin` - WebAuthn setup
- ✅ `POST /api/v1/identity/mfa/webauthn/register/complete` - Complete WebAuthn

---

## 💳 Wallet Service API (`/api/v1/wallet`)

### Available Endpoints

#### Credential Issuance (OpenID4VCI)
- ✅ `POST /api/v1/wallet/credential` - Request credential
- ✅ `POST /api/v1/wallet/batch_credential` - Request multiple credentials
- ✅ `POST /api/v1/wallet/Credential/issue` - Issue credential
- ✅ `GET /.well-known/openid-credential-issuer` - Credential issuer metadata

#### Credential Management (Holder)
- ✅ `GET /api/v1/wallet/Holder` - Get holder info
- ✅ `GET /api/v1/wallet/Holder/credential/{credentialId}` - Get credential
- ✅ `POST /api/v1/wallet/Holder/present/{credentialId}` - Present credential
- ✅ `GET /api/v1/wallet/Credential/holder/{holderId}` - Get holder's credentials

#### Credential Verification (Verifier)
- ✅ `POST /api/v1/wallet/Verifier/verify` - Verify credential
- ✅ `POST /api/v1/wallet/Verifier/verify-presentation` - Verify VP
- ✅ `POST /api/v1/wallet/Verifier/verify/authorize` - Authorize verification
- ✅ `POST /api/v1/wallet/Verifier/verify/vp-token` - Verify VP token

#### Credential Lifecycle
- ✅ `GET /api/v1/wallet/Credential/{credentialId}` - Get credential details
- ✅ `POST /api/v1/wallet/Credential/{credentialId}/revoke` - Revoke credential
- ✅ `POST /api/v1/wallet/Credential/{credentialId}/suspend` - Suspend credential
- ✅ `POST /api/v1/wallet/Credential/{credentialId}/reactivate` - Reactivate credential
- ✅ `GET /api/v1/wallet/Credential/{credentialId}/status` - Get credential status

#### Workflows (Citizen-Centric)
- ✅ `GET /api/v1/wallet/Workflow/citizens/{citizenSub}/credentials` - Get citizen's credentials
- ✅ `GET /api/v1/wallet/Workflow/credentials/{credentialId}` - Get workflow credential
- ✅ `POST /api/v1/wallet/Workflow/issue` - Issue via workflow

#### Device Transfer
- ✅ `POST /api/v1/wallet/DeviceTransfer/export` - Export wallet data
- ✅ `POST /api/v1/wallet/DeviceTransfer/import` - Import wallet data
- ✅ `POST /api/v1/wallet/DeviceTransfer/validate` - Validate transfer

---

## 📱 CredVault Mobile App Current State

### ✅ What's Working

1. **WebAuthenticator Configuration**
   - `WebAuthenticatorCallbackActivity` properly configured for Android
   - Callback URL: `credvault://oauth-callback`
   - Intent filter registered for `credvault` scheme
   - MainActivity includes Platform initialization

2. **Token Storage**
   - Uses `SecureStorage.SetAsync("access_token", token)`
   - Secure storage implementation exists
   - No plaintext storage of credentials

3. **Navigation Architecture**
   - `INavigationService` abstraction implemented
   - Shell-based routing configured
   - DI container properly set up

4. **Service Layer**
   - `AuthenticationFlowService` implements OAuth2 flow
   - `IWalletService` interface exists
   - ViewModel architecture with MVVM toolkit

### ❌ Critical Gaps

#### 1. **Missing Bearer Token Injection** 🚨
**Impact:** HIGH - API calls will fail with 401 Unauthorized

**Current State:**
```csharp
// MauiProgram.cs - NO Authorization header handling
services.AddRefitClient<IWalletApiClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl));
```

**Required Fix:**
```csharp
// NEEDED: AuthHeaderHandler.cs
public class AuthHeaderHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await SecureStorage.GetAsync("access_token");
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        
        return await base.SendAsync(request, cancellationToken);
    }
}

// MauiProgram.cs
services.AddTransient<AuthHeaderHandler>();
services.AddRefitClient<IWalletApiClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl))
    .AddHttpMessageHandler<AuthHeaderHandler>(); // ← ADD THIS
```

#### 2. **API Client Mismatch** 🚨
**Impact:** HIGH - Endpoints don't match backend

**Current Interface:**
```csharp
// src/CredVault.Mobile/Services/IWalletApiClient.cs
public interface IWalletApiClient
{
    [Post("/api/credentials/initiate")]
    Task<ApiResponse<InitiateCredentialResponse>> InitiateCredentialIssuance(
        [Body] InitiateCredentialRequest request);
    
    [Post("/api/credentials/exchange-code")]
    Task<ApiResponse<TokenResponse>> ExchangeAuthorizationCode(
        [Body] ExchangeCodeRequest request);
}
```

**Should Be:**
```csharp
public interface IWalletApiClient
{
    // OpenID4VCI flow
    [Post("/api/v1/wallet/Authorization/authorize")]
    Task<ApiResponse<AuthorizationResponse>> InitiateCredentialIssuance(
        [Body] AuthorizationRequest request);
    
    [Post("/api/v1/wallet/Authorization/token")]
    Task<ApiResponse<TokenResponse>> ExchangeAuthorizationCode(
        [Body] TokenRequest request);
    
    [Post("/api/v1/wallet/credential")]
    Task<ApiResponse<CredentialResponse>> RequestCredential(
        [Body] CredentialRequest request,
        [Header("Authorization")] string authorization);
    
    // Holder endpoints
    [Get("/api/v1/wallet/Credential/holder/{holderId}")]
    Task<ApiResponse<List<CredentialDto>>> GetHolderCredentials(
        string holderId);
    
    [Post("/api/v1/wallet/Holder/present/{credentialId}")]
    Task<ApiResponse<PresentationResponse>> PresentCredential(
        string credentialId,
        [Body] PresentationRequest request);
    
    // Verification
    [Post("/api/v1/wallet/Verifier/verify-presentation")]
    Task<ApiResponse<VerificationResult>> VerifyPresentation(
        [Body] VerifyPresentationRequest request);
}
```

#### 3. **Missing Identity Client** 🚨
**Impact:** HIGH - Can't register users or refresh tokens

**Needed:**
```csharp
public interface IIdentityApiClient
{
    [Post("/api/v1/identity/auth/register")]
    Task<ApiResponse<RegisterResponse>> RegisterUser(
        [Body] RegisterRequest request);
    
    [Post("/api/v1/identity/auth/login")]
    Task<ApiResponse<LoginResponse>> Login(
        [Body] LoginRequest request);
    
    [Post("/api/v1/identity/auth/refresh")]
    Task<ApiResponse<TokenResponse>> RefreshToken(
        [Body] RefreshTokenRequest request);
    
    [Get("/api/v1/identity/users/profile")]
    Task<ApiResponse<UserProfileDto>> GetProfile();
    
    [Put("/api/v1/identity/users/profile")]
    Task<ApiResponse<UserProfileDto>> UpdateProfile(
        [Body] UpdateProfileRequest request);
    
    [Post("/api/v1/identity/mfa/totp/setup")]
    Task<ApiResponse<TotpSetupResponse>> SetupTotp();
    
    [Post("/api/v1/identity/mfa/verify")]
    Task<ApiResponse<MfaVerifyResponse>> VerifyMfa(
        [Body] MfaVerifyRequest request);
}
```

#### 4. **No Biometric Authentication**
**Impact:** MEDIUM - Security best practice missing

Currently no biometric verification before presenting credentials to verifiers.

**Needed:**
```csharp
// NEEDED: Before credential presentations
var authenticated = await BiometricAuthenticator.AuthenticateAsync(
    new AuthenticationRequestConfiguration(
        "Verify Your Identity",
        "Authenticate to share your credential"));

if (!authenticated)
    return; // Cancel presentation
```

#### 5. **No Token Refresh Logic**
**Impact:** MEDIUM - Users will need to re-login frequently

**Needed:**
```csharp
// NEEDED: In AuthHeaderHandler or separate RefreshTokenHandler
if (response.StatusCode == HttpStatusCode.Unauthorized)
{
    var refreshToken = await SecureStorage.GetAsync("refresh_token");
    if (!string.IsNullOrEmpty(refreshToken))
    {
        var tokenResponse = await _identityClient.RefreshToken(
            new RefreshTokenRequest { RefreshToken = refreshToken });
        
        await SecureStorage.SetAsync("access_token", 
            tokenResponse.AccessToken);
        
        // Retry original request
        request.Headers.Authorization = 
            new AuthenticationHeaderValue("Bearer", 
                tokenResponse.AccessToken);
        response = await base.SendAsync(request, cancellationToken);
    }
}
```

#### 6. **iOS Configuration Missing**
**Impact:** MEDIUM - App won't work on iOS

**Needed: `Platforms/iOS/Info.plist`**
```xml
<key>CFBundleURLTypes</key>
<array>
    <dict>
        <key>CFBundleURLSchemes</key>
        <array>
            <string>credvault</string>
        </array>
        <key>CFBundleURLName</key>
        <string>com.credvault.mobile</string>
    </dict>
</array>
```

---

## 📊 Integration Compliance Matrix

| Requirement | Status | Implementation | Backend Endpoint |
|-------------|--------|----------------|------------------|
| **Authentication** |
| User Registration | ❌ Missing | Need IIdentityApiClient | `POST /api/v1/identity/auth/register` |
| User Login | ⚠️ Partial | Uses WebAuthenticator | `GET /connect/authorize` |
| Token Exchange | ⚠️ Partial | Incorrect endpoint | `POST /connect/token` |
| Token Refresh | ❌ Missing | No implementation | `POST /api/v1/identity/auth/refresh` |
| User Profile | ⚠️ Stub | IWalletService.GetUserProfileAsync | `GET /api/v1/identity/users/profile` |
| **Authorization** |
| Bearer Token Injection | ❌ Missing | No AuthHeaderHandler | N/A (all protected endpoints) |
| Token Storage | ✅ Complete | SecureStorage | N/A |
| OIDC Discovery | ❌ Not Used | Hardcoded URLs | `GET /.well-known/openid-configuration` |
| **Credential Issuance** |
| Initiate Issuance | ⚠️ Wrong URL | `/api/credentials/initiate` | `POST /api/v1/wallet/Authorization/authorize` |
| Get Access Token | ⚠️ Wrong URL | `/api/credentials/exchange-code` | `POST /api/v1/wallet/Authorization/token` |
| Request Credential | ❌ Missing | No endpoint | `POST /api/v1/wallet/credential` |
| Batch Request | ❌ Missing | No endpoint | `POST /api/v1/wallet/batch_credential` |
| **Credential Management** |
| List Credentials | ⚠️ Local Only | No backend sync | `GET /api/v1/wallet/Credential/holder/{holderId}` |
| Get Credential Details | ❌ Missing | No endpoint | `GET /api/v1/wallet/Credential/{credentialId}` |
| Check Status | ❌ Missing | No endpoint | `GET /api/v1/wallet/Credential/{credentialId}/status` |
| **Credential Presentation** |
| Present Credential | ⚠️ Partial | QR code only | `POST /api/v1/wallet/Holder/present/{credentialId}` |
| Selective Disclosure | ⚠️ UI Only | No backend call | Part of presentation flow |
| Biometric Auth | ❌ Missing | No implementation | N/A (client-side) |
| **Verification** |
| Scan QR | ✅ Complete | ZXing.Net.Maui | N/A (client-side) |
| Verify Presentation | ❌ Missing | No endpoint | `POST /api/v1/wallet/Verifier/verify-presentation` |
| Verify Credential | ❌ Missing | No endpoint | `POST /api/v1/wallet/Verifier/verify` |
| **Security** |
| HTTPS Enforcement | ⚠️ Unknown | No cert validation | N/A |
| PKCE | ❌ Not Used | Standard OAuth2 | N/A |
| MFA Support | ❌ Missing | No endpoints | `POST /api/v1/identity/mfa/verify` |
| **Device Management** |
| Backup Credentials | ❌ Missing | No endpoint | `POST /api/v1/wallet/DeviceTransfer/export` |
| Restore Credentials | ❌ Missing | No endpoint | `POST /api/v1/wallet/DeviceTransfer/import` |

**Score: 7/34 Complete (20.6%)** 🔴

---

## 🚀 Recommended Implementation Plan

### Phase 1: Critical Fixes (Week 1)
**Priority: URGENT** - App currently non-functional with backend

1. ✅ **Already Done:** WebAuthenticator callback activity
2. **Create AuthHeaderHandler** - Enable Bearer token injection
3. **Create IIdentityApiClient** - Connect to Identity service
4. **Update IWalletApiClient** - Fix endpoint URLs
5. **Register HTTP handlers** - Wire up in MauiProgram.cs
6. **Test end-to-end** - Register → Login → Get Credentials

**Deliverables:**
- [ ] `Services/AuthHeaderHandler.cs`
- [ ] `Services/IIdentityApiClient.cs`
- [ ] Updated `Services/IWalletApiClient.cs`
- [ ] Updated `MauiProgram.ConfigureApiClients()`
- [ ] iOS `Info.plist` URL scheme configuration

### Phase 2: Core Features (Week 2)
**Priority: HIGH** - Essential wallet functionality

1. **Implement Token Refresh** - Auto-refresh on 401
2. **Add Biometric Auth** - Before credential presentations
3. **Credential Sync** - Fetch from `/api/v1/wallet/Credential/holder/{holderId}`
4. **Status Checks** - Poll credential status
5. **Error Handling** - User-friendly error messages

**Deliverables:**
- [ ] `Services/RefreshTokenHandler.cs`
- [ ] Biometric authentication helper
- [ ] Background credential sync
- [ ] Status badge UI updates

### Phase 3: Advanced Features (Week 3)
**Priority: MEDIUM** - Enhanced user experience

1. **MFA Support** - TOTP setup and verification
2. **Device Transfer** - Backup/restore credentials
3. **Credential Revocation** - User-initiated revocation
4. **Audit Logs** - Display credential usage history
5. **PKCE Implementation** - Enhanced OAuth2 security

**Deliverables:**
- [ ] MFA setup flow
- [ ] Backup/restore UI
- [ ] Credential lifecycle management
- [ ] Audit log viewer

### Phase 4: Testing & Validation (Week 4)
**Priority: MEDIUM** - Ensure reliability

1. **Integration Tests** - Test against live backend
2. **Error Scenarios** - Network failures, expired tokens
3. **Performance Testing** - Measure API latency
4. **Security Audit** - Token handling, storage
5. **User Acceptance Testing** - Real-world scenarios

---

## 🔧 Quick Start: Fix Critical Issues

### Step 1: Create AuthHeaderHandler

```csharp
// src/CredVault.Mobile/Services/AuthHeaderHandler.cs
using System.Net.Http.Headers;

namespace CredVault.Mobile.Services;

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
                request.Headers.Authorization = 
                    new AuthenticationHeaderValue("Bearer", token);
                _logger.LogDebug("Added Bearer token to request: {Url}", 
                    request.RequestUri);
            }
            else
            {
                _logger.LogWarning("No access token found for request: {Url}", 
                    request.RequestUri);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding auth header");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
```

### Step 2: Create IIdentityApiClient

```csharp
// src/CredVault.Mobile/Services/IIdentityApiClient.cs
using Refit;

namespace CredVault.Mobile.Services;

public interface IIdentityApiClient
{
    [Post("/api/v1/identity/auth/register")]
    Task<ApiResponse<RegisterResponse>> RegisterUser(
        [Body] RegisterRequest request);

    [Post("/api/v1/identity/auth/login")]
    Task<ApiResponse<LoginResponse>> Login(
        [Body] LoginRequest request);

    [Post("/api/v1/identity/auth/refresh")]
    Task<ApiResponse<TokenResponse>> RefreshToken(
        [Body] RefreshTokenRequest request);

    [Get("/api/v1/identity/users/profile")]
    Task<ApiResponse<UserProfileDto>> GetProfile();

    [Put("/api/v1/identity/users/profile")]
    Task<ApiResponse<UserProfileDto>> UpdateProfile(
        [Body] UpdateProfileRequest request);
}

// DTOs
public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName);

public record RegisterResponse(
    Guid UserId,
    string Username,
    string Email);

public record LoginRequest(string Username, string Password);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string TokenType);

public record RefreshTokenRequest(string RefreshToken);

public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn);

public record UserProfileDto(
    Guid Id,
    string Username,
    string Email,
    string? FirstName,
    string? LastName,
    bool IsEmailVerified);

public record UpdateProfileRequest(
    string? FirstName,
    string? LastName,
    string? PhoneNumber);
```

### Step 3: Update MauiProgram.cs

```csharp
// src/CredVault.Mobile/MauiProgram.cs
private static void ConfigureApiClients(IServiceCollection services)
{
    var baseUrl = DeviceInfo.Platform == DevicePlatform.Android
        ? "http://10.0.2.2:8000"  // Android emulator
        : "http://localhost:8000";

    // Register AuthHeaderHandler
    services.AddTransient<AuthHeaderHandler>();

    // Identity API Client
    services.AddRefitClient<IIdentityApiClient>()
        .ConfigureHttpClient(c =>
        {
            c.BaseAddress = new Uri(baseUrl);
            c.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<AuthHeaderHandler>();

    // Wallet API Client (updated)
    services.AddRefitClient<IWalletApiClient>()
        .ConfigureHttpClient(c =>
        {
            c.BaseAddress = new Uri(baseUrl);
            c.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<AuthHeaderHandler>();
}
```

### Step 4: Update IWalletApiClient

```csharp
// src/CredVault.Mobile/Services/IWalletApiClient.cs
public interface IWalletApiClient
{
    // Authorization (OpenID4VCI)
    [Post("/api/v1/wallet/Authorization/authorize")]
    Task<ApiResponse<AuthorizationResponse>> InitiateCredentialIssuance(
        [Body] AuthorizationRequest request);

    [Post("/api/v1/wallet/Authorization/token")]
    Task<ApiResponse<TokenResponse>> ExchangeAuthorizationCode(
        [Body] TokenRequest request);

    // Credential Operations
    [Post("/api/v1/wallet/credential")]
    Task<ApiResponse<CredentialResponse>> RequestCredential(
        [Body] CredentialRequest request);

    [Get("/api/v1/wallet/Credential/holder/{holderId}")]
    Task<ApiResponse<List<CredentialDto>>> GetHolderCredentials(
        string holderId);

    [Get("/api/v1/wallet/Credential/{credentialId}")]
    Task<ApiResponse<CredentialDto>> GetCredential(string credentialId);

    [Get("/api/v1/wallet/Credential/{credentialId}/status")]
    Task<ApiResponse<CredentialStatusDto>> GetCredentialStatus(
        string credentialId);

    // Presentation
    [Post("/api/v1/wallet/Holder/present/{credentialId}")]
    Task<ApiResponse<PresentationResponse>> PresentCredential(
        string credentialId,
        [Body] PresentationRequest request);

    // Verification
    [Post("/api/v1/wallet/Verifier/verify-presentation")]
    Task<ApiResponse<VerificationResult>> VerifyPresentation(
        [Body] VerifyPresentationRequest request);
}
```

---

## 📈 Success Metrics

### Week 1 Targets
- [ ] 100% of API calls include Bearer token
- [ ] User can register via `/api/v1/identity/auth/register`
- [ ] User can login and receive access token
- [ ] Credentials fetched from `/api/v1/wallet/Credential/holder/{holderId}`

### Week 2 Targets
- [ ] Token auto-refresh on 401 response
- [ ] Biometric auth before credential sharing
- [ ] All credentials show real-time status
- [ ] < 2s average API response time

### Week 3 Targets
- [ ] MFA setup functional
- [ ] Backup/restore tested successfully
- [ ] Credential lifecycle (issue → present → revoke) working
- [ ] Audit logs displayed in Settings

### Week 4 Targets
- [ ] 90%+ integration test coverage
- [ ] Zero crashes in error scenarios
- [ ] User acceptance criteria met
- [ ] Performance benchmarks achieved

---

## 📞 Next Steps

1. **Review this report** with development team
2. **Prioritize fixes** - Start with AuthHeaderHandler
3. **Set up test environment** - Ensure backend is accessible from emulator
4. **Create feature branches** - One per critical fix
5. **Implement Phase 1** - Target completion by end of Week 1
6. **Schedule code reviews** - After each phase completion

---

**Report Generated:** November 2, 2025  
**Backend Version:** Identity v1, Wallet v1  
**Mobile App Version:** CredVault.Mobile (net9.0-android/ios)  
**Prepared by:** GitHub Copilot
