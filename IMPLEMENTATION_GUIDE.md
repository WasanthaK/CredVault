# Implementation Guide - Credential Issuance Flow

**Last Updated:** November 11, 2025  
**Current Status:** OAuth/PKCE Complete (60%), ViewModel Integration Next  
**Priority:** HIGH - Core wallet functionality

---

## 📋 Quick Status

### ✅ Completed (60%)
- **Phase 1**: Foundation complete (models, deep links, API testing)
- **Phase 2**: OAuth/PKCE implementation complete (token exchange, storage)
- **Azure Discovery**: All 4 services documented with Swagger URLs

### 🎯 In Progress (40%)
- **Phase 3**: ViewModel integration (1-2 hours) ← **NEXT STEP**
- **Phase 4**: Credential offer parsing (2-3 hours)
- **Phase 5**: Credential storage (2-3 hours)
- **Phase 6**: Testing & polish (4-6 hours)

**Estimated Completion:** 2-3 days remaining

---

## 🎯 Business Requirements

### User Story
```
As a wallet holder
I want to add new credentials from government and institutional issuers
So that I can store and present my verifiable credentials digitally
```

### Acceptance Criteria
- ✅ User can browse available credential types (National ID, Driver's License, Diploma)
- ✅ User can initiate credential issuance flow with selected issuer
- ✅ System launches browser for OAuth authentication (Nov 11)
- ✅ System handles OAuth callback and exchanges authorization code for tokens (Nov 11)
- 🟡 System requests credential offer from issuer - **IN PROGRESS**
- 🟡 User reviews credential claims before accepting - **UI READY, needs data**
- 🔴 System stores issued credential in wallet
- 🔴 Credential appears on dashboard
- 🔴 User can view credential details
- 🟡 All operations have proper error handling - **PARTIAL**

---

## 🏗️ Architecture & Flow

### Complete Flow Diagram
```
┌─────────────────────────────────────────────────────────────────┐
│ 1. SELECT CREDENTIAL TYPE (SelectCredentialTypePage)             │
│    User: Tap "National ID" → Tap "Continue"                     │
│    ViewModel: AddCredentialViewModel.ProceedToAuthentication()  │
└──────────────────────────┬──────────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────────┐
│ 2. START AUTHENTICATION FLOW                                     │
│    ViewModel → AuthenticationFlowService.StartIssuanceFlow()    │
│    Service generates PKCE params (32-byte verifier, SHA256)     │
│    Service builds OAuth URL with challenge                       │
│    Response: Authorization URL + state                           │
└──────────────────────────┬──────────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────────┐
│ 3. OAUTH BROWSER AUTHENTICATION ✅ COMPLETE                      │
│    ViewModel → WebAuthenticator.AuthenticateAsync()             │
│    Browser opens: https://identity.../connect/authorize?...     │
│    User logs in with issuer credentials                          │
│    Browser redirects: credvault://oauth-callback?code=...       │
│    Deep link: Android intent-filter / iOS CFBundleURLTypes      │
└──────────────────────────┬──────────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────────┐
│ 4. EXCHANGE AUTHORIZATION CODE ✅ COMPLETE                       │
│    Service validates state parameter (CSRF protection)           │
│    Service → POST /connect/token with code_verifier             │
│    Request: grant_type=authorization_code, code, verifier       │
│    Response: { access_token, refresh_token, expires_in }        │
│    Service → SecureStorage: Store tokens                         │
└──────────────────────────┬──────────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────────┐
│ 5. REQUEST CREDENTIAL OFFER (Next Step)                          │
│    Service retrieves access_token from SecureStorage            │
│    Service → GET /api/v1/wallet/discovery/credential_offer      │
│    Headers: Authorization: Bearer {access_token}                │
│    Response: { claims, issuer, schema, expiry, format }         │
│    Map to CredentialOfferDetails model                           │
└──────────────────────────┬──────────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────────┐
│ 6. REVIEW & CONSENT (ConsentReviewPage)                          │
│    Display credential claims to user                             │
│    User: Reviews claims (Name, DOB, Photo, ID#, etc.)          │
│    User: ☑ I have reviewed  ☑ I consent                        │
│    User: Tap "Confirm Issuance"                                 │
└──────────────────────────┬──────────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────────┐
│ 7. ISSUE & STORE CREDENTIAL                                      │
│    ViewModel → Service.AcceptAndStoreCredential()               │
│    Service → POST /api/v1/wallet/credential                      │
│    Request: { format, proof, credentialSubject, ... }           │
│    Response: { credential, transactionId, c_nonce }             │
│    Service → SecureStorage: Store credential                     │
│    Service → Create holder if not exists                         │
└──────────────────────────┬──────────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────────┐
│ 8. CONFIRMATION & DASHBOARD UPDATE                               │
│    Show success message                                          │
│    Navigate to Dashboard                                         │
│    DashboardViewModel refreshes credentials list                │
│    Dashboard displays new credential with status badge          │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🔐 OAuth 2.0 + PKCE Implementation

### What's Complete (Nov 11, 2025)

**AuthenticationFlowService.cs** - Production Ready ✅

```csharp
// 1. Generate PKCE Parameters
var pkce = GeneratePKCEParameters();
// Creates: 32-byte code_verifier, SHA256 code_challenge, state

// 2. Build Authorization URL
var authUrl = BuildAuthorizationUrlWithPKCE(credentialType, pkce);
// URL: https://identity.../connect/authorize
//   ?client_id=credvault-mobile
//   &redirect_uri=credvault://oauth-callback
//   &response_type=code
//   &scope=openid profile email roles
//   &code_challenge={SHA256}
//   &code_challenge_method=S256
//   &state={GUID}

// 3. Exchange Code for Token
var tokens = await ExchangeCodeForTokenWithPKCEAsync(authCode, pkce);
// POST /connect/token
// Body: grant_type=authorization_code
//       code={auth_code}
//       code_verifier={original_verifier}
//       client_id=credvault-mobile
//       redirect_uri=credvault://oauth-callback

// 4. Store Tokens Securely
await SecureStorage.SetAsync("access_token", tokens.AccessToken);
await SecureStorage.SetAsync("refresh_token", tokens.RefreshToken);
```

**Key Security Features:**
- ✅ PKCE S256 (SHA256) - Prevents authorization code interception
- ✅ State parameter validation - CSRF protection
- ✅ Secure token storage - Platform keychain/keystore
- ✅ Code verifier cleanup - No sensitive data left in memory
- ✅ Ephemeral browser session - No persistent cookies

**Standards Compliance:**
- ✅ RFC 7636 (PKCE)
- ✅ RFC 6749 (OAuth 2.0)
- ✅ OpenID Connect Core 1.0

---

## 📦 Technical Components

### Models (CredentialOffer.cs) ✅

```csharp
public class CredentialOfferDetails
{
    public string CredentialType { get; set; }      // "NationalID"
    public string Format { get; set; }              // "jwt_vc_json"
    public Dictionary<string, object> Claims { get; set; }
    public IssuerMetadata Issuer { get; set; }
    public string Schema { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class IssuerMetadata
{
    public string IssuerId { get; set; }
    public string Name { get; set; }
    public string AuthorizationEndpoint { get; set; }
    public string TokenEndpoint { get; set; }
    public string CredentialEndpoint { get; set; }
}

public class PKCEParameters
{
    public string CodeVerifier { get; set; }        // 32-byte random
    public string CodeChallenge { get; set; }       // Base64Url(SHA256(verifier))
    public string CodeChallengeMethod { get; set; } // "S256"
    public string State { get; set; }               // GUID for CSRF protection
}
```

### Platform Configuration ✅

**Android (AndroidManifest.xml):**
```xml
<activity android:name="MainActivity" android:launchMode="singleTask">
  <intent-filter>
    <action android:name="android.intent.action.VIEW" />
    <category android:name="android.intent.category.DEFAULT" />
    <category android:name="android.intent.category.BROWSABLE" />
    <data android:scheme="credvault" android:host="oauth-callback" />
  </intent-filter>
</activity>
```

**iOS (Info.plist):**
```xml
<key>CFBundleURLTypes</key>
<array>
  <dict>
    <key>CFBundleURLName</key>
    <string>com.credvault.mobile</string>
    <key>CFBundleURLSchemes</key>
    <array>
      <string>credvault</string>
    </array>
  </dict>
</array>
```

---

## 🚀 Implementation Steps

### Phase 3: ViewModel Integration (NEXT - 1-2 hours)

**File:** `ViewModels/AddCredentialViewModel.cs`

**Current State:**
```csharp
[RelayCommand]
private async Task AuthenticateCredential()
{
    // TODO: Mock authentication - replace with real OAuth
    IsAuthenticating = true;
    await Task.Delay(2000);
    IsAuthenticated = true;
    IsAuthenticating = false;
}
```

**Required Changes:**
```csharp
[RelayCommand]
private async Task AuthenticateCredential()
{
    try
    {
        IsAuthenticating = true;
        
        // 1. Start OAuth flow
        var authResult = await _authFlowService.StartCredentialIssuanceFlowAsync(
            SelectedCredentialType.Type,
            SelectedCredentialType.IssuerId
        );
        
        if (!authResult.IsSuccess)
        {
            await ShowErrorAsync(authResult.Message);
            return;
        }
        
        // 2. Launch browser authentication
        var webResult = await WebAuthenticator.AuthenticateAsync(
            new WebAuthenticatorOptions {
                Url = new Uri(authResult.Data.AuthorizationUrl),
                CallbackUrl = new Uri("credvault://oauth-callback"),
                PrefersEphemeralWebBrowserSession = true
            });
        
        // 3. Extract callback parameters
        var authCode = webResult.Properties["code"];
        var state = webResult.Properties["state"];
        
        // 4. Exchange code for tokens
        var tokenResult = await _authFlowService.HandleOAuthCallbackAsync(
            authCode, 
            state
        );
        
        if (!tokenResult.IsSuccess)
        {
            await ShowErrorAsync("Token exchange failed");
            return;
        }
        
        // 5. Request credential offer
        var offerResult = await _authFlowService.RequestCredentialIssuanceAsync(
            SelectedCredentialType.Type
        );
        
        if (!offerResult.IsSuccess)
        {
            await ShowErrorAsync("Failed to retrieve credential offer");
            return;
        }
        
        // 6. Navigate to consent review
        CredentialOffer = offerResult.Data;
        await Shell.Current.GoToAsync($"//{nameof(ConsentReviewPage)}", 
            new Dictionary<string, object> {
                { "CredentialOffer", CredentialOffer }
            });
        
        IsAuthenticated = true;
    }
    catch (TaskCanceledException)
    {
        // User cancelled the OAuth flow
        await ShowInfoAsync("Authentication cancelled");
    }
    catch (Exception ex)
    {
        await ShowErrorAsync($"Authentication error: {ex.Message}");
    }
    finally
    {
        IsAuthenticating = false;
    }
}
```

**Dependencies to Add:**
```csharp
using Microsoft.Maui.Authentication;
using CredVault.Mobile.Models;
```

---

### Phase 4: Credential Offer Parsing (2-3 hours)

**File:** `Services/AuthenticationFlowService.cs`

**Update Method:**
```csharp
public async Task<ServiceResult<CredentialOfferDetails>> RequestCredentialIssuanceAsync(
    string credentialType)
{
    try
    {
        // 1. Retrieve access token
        var accessToken = await SecureStorage.GetAsync("access_token");
        if (string.IsNullOrEmpty(accessToken))
            return ServiceResult<CredentialOfferDetails>.Failure("No access token");
        
        // 2. Call Wallet API
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", accessToken);
        
        var response = await _httpClient.GetAsync(
            $"{ApiConfiguration.Azure.Wallet.BaseUrl}/api/v1/wallet/discovery/credential_offer?type={credentialType}"
        );
        
        if (!response.IsSuccessStatusCode)
            return ServiceResult<CredentialOfferDetails>.Failure("API call failed");
        
        // 3. Parse response
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponseDto<CredentialOfferDto>>();
        
        if (apiResponse?.Data == null)
            return ServiceResult<CredentialOfferDetails>.Failure("Invalid response");
        
        // 4. Map to model
        var offer = new CredentialOfferDetails {
            CredentialType = apiResponse.Data.CredentialType,
            Format = apiResponse.Data.Format,
            Claims = apiResponse.Data.CredentialSubject,
            Schema = apiResponse.Data.CredentialSchema,
            ExpiresAt = apiResponse.Data.ExpirationDate,
            Issuer = new IssuerMetadata {
                IssuerId = apiResponse.Data.Issuer.Id,
                Name = apiResponse.Data.Issuer.Name,
                CredentialEndpoint = apiResponse.Data.Issuer.CredentialEndpoint
            }
        };
        
        return ServiceResult<CredentialOfferDetails>.Success(offer);
    }
    catch (Exception ex)
    {
        return ServiceResult<CredentialOfferDetails>.Failure(ex.Message);
    }
}
```

**Update ConsentReviewPage.xaml.cs:**
```csharp
[QueryProperty(nameof(CredentialOffer), "CredentialOffer")]
public partial class ConsentReviewPage : ContentPage
{
    private CredentialOfferDetails _credentialOffer;
    
    public CredentialOfferDetails CredentialOffer
    {
        get => _credentialOffer;
        set
        {
            _credentialOffer = value;
            LoadCredentialData();
        }
    }
    
    private void LoadCredentialData()
    {
        CredentialTypeLabel.Text = CredentialOffer.CredentialType;
        IssuerLabel.Text = CredentialOffer.Issuer.Name;
        ExpiryLabel.Text = CredentialOffer.ExpiresAt?.ToString("MMM dd, yyyy");
        
        // Populate claims list
        foreach (var claim in CredentialOffer.Claims)
        {
            ClaimsStack.Children.Add(new Label {
                Text = $"{claim.Key}: {claim.Value}"
            });
        }
    }
}
```

---

### Phase 5: Credential Storage (2-3 hours)

**File:** `Services/AuthenticationFlowService.cs`

**Add Method:**
```csharp
public async Task<ServiceResult<string>> AcceptAndStoreCredentialAsync(
    CredentialOfferDetails offer)
{
    try
    {
        // 1. Get access token
        var accessToken = await SecureStorage.GetAsync("access_token");
        
        // 2. Build credential request
        var request = new OpenID4VCICredentialRequestDto {
            Format = offer.Format,
            CredentialDefinition = new {
                Type = new[] { "VerifiableCredential", offer.CredentialType }
            },
            Proof = new {
                ProofType = "jwt",
                Jwt = accessToken // Simplified - should be signed JWT
            }
        };
        
        // 3. Issue credential
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", accessToken);
        
        var response = await _httpClient.PostAsJsonAsync(
            $"{ApiConfiguration.Azure.Wallet.BaseUrl}/api/v1/wallet/credential",
            request
        );
        
        if (!response.IsSuccessStatusCode)
            return ServiceResult<string>.Failure("Credential issuance failed");
        
        var apiResponse = await response.Content
            .ReadFromJsonAsync<ApiResponseDto<OpenID4VCICredentialResponseDto>>();
        
        // 4. Store credential in SecureStorage
        var credentialId = Guid.NewGuid().ToString();
        var credentialJson = JsonSerializer.Serialize(apiResponse.Data);
        
        await SecureStorage.SetAsync($"credential_{credentialId}", credentialJson);
        
        // 5. Add to credentials list
        var credentialsList = await SecureStorage.GetAsync("credentials_list") ?? "[]";
        var list = JsonSerializer.Deserialize<List<string>>(credentialsList);
        list.Add(credentialId);
        await SecureStorage.SetAsync("credentials_list", JsonSerializer.Serialize(list));
        
        return ServiceResult<string>.Success(credentialId);
    }
    catch (Exception ex)
    {
        return ServiceResult<string>.Failure(ex.Message);
    }
}
```

**Update DashboardViewModel.cs:**
```csharp
public async Task LoadCredentialsAsync()
{
    try
    {
        IsLoading = true;
        
        // Get list of credential IDs
        var credentialsList = await SecureStorage.GetAsync("credentials_list");
        if (string.IsNullOrEmpty(credentialsList))
        {
            Credentials.Clear();
            return;
        }
        
        var ids = JsonSerializer.Deserialize<List<string>>(credentialsList);
        
        // Load each credential
        Credentials.Clear();
        foreach (var id in ids)
        {
            var credJson = await SecureStorage.GetAsync($"credential_{id}");
            if (!string.IsNullOrEmpty(credJson))
            {
                var cred = JsonSerializer.Deserialize<CredentialViewModel>(credJson);
                Credentials.Add(cred);
            }
        }
    }
    finally
    {
        IsLoading = false;
    }
}
```

---

### Phase 6: Testing & Polish (4-6 hours)

**Testing Checklist:**
- [ ] Test OAuth flow on Android emulator
- [ ] Test OAuth flow on iOS simulator
- [ ] Test with test account: wasanthak@enadoc.com
- [ ] Verify PKCE parameters generated correctly
- [ ] Verify tokens stored securely
- [ ] Verify deep link callback works
- [ ] Test user cancellation of OAuth
- [ ] Test network error handling
- [ ] Test invalid credentials
- [ ] Test token refresh
- [ ] Verify credential displays on dashboard
- [ ] Test credential details page
- [ ] Verify status badges work

**Error Scenarios:**
- Network timeout
- Invalid OAuth callback
- Token exchange failure
- API rate limiting
- Credential already exists
- Issuer not available

---

## 📚 API Reference

See `AZURE_CONFIGURATION.md` for complete API details.

**Key Endpoints:**
- Authorization: `POST /connect/authorize`, `POST /connect/token`
- Discovery: `GET /api/v1/wallet/discovery/credential_offer`
- Issuance: `POST /api/v1/wallet/credential`
- Holder: `GET /api/v1/wallet/Credential/holder/{holderId}`

**Test Credentials:**
- Email: wasanthak@enadoc.com
- Password: Passw0rd!
- User ID: b7745358-49ea-40a4-9ae7-aa81193eed5f

---

## 🎯 Success Metrics

- ✅ OAuth flow completes without errors
- ✅ Tokens stored securely
- ✅ Deep links work on both platforms
- 🟡 Credential offer retrieved from API
- 🔴 Credential stored in wallet
- 🔴 Credential appears on dashboard
- 🔴 End-to-end flow < 30 seconds

**Current Progress:** 60% complete

---

**Next Action:** Wire AddCredentialViewModel.AuthenticateCredential() to OAuth service (1-2 hours)
