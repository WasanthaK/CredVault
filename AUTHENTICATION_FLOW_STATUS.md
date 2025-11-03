# CredVault Authentication & Credential Issuance Flow

## 🔴 Current Status: MOCK IMPLEMENTATION

The current implementation has **UI pages** but uses **simulated/mock data** instead of real OAuth/OpenID4VCI flows.

---

## ✅ Proper Implementation Flow (OpenID4VCI Standard)

### **Step 1: User Selects Credential Type**
**Page:** `SelectCredentialTypePage`
**Status:** ✅ COMPLETE (UI working)

```
User taps "Add Credential" → Sees 3 options:
- National ID (Gov ID Authority)
- Driver's License (DMV)
- University Diploma (State University)
→ User selects one → Taps "Continue"
```

---

### **Step 2: Start Authentication Flow** ⚠️ NEEDS IMPLEMENTATION
**What should happen:**

```csharp
// In AddCredentialViewModel.ProceedToAuthenticationAsync()

// 1. Get issuer metadata and authorization URL
var authFlowService = new AuthenticationFlowService(...);
var authUrlResult = await authFlowService.StartCredentialIssuanceFlowAsync(
    credentialType: "NationalID",
    issuerId: "gov-id-authority"
);

// 2. Launch platform WebAuthenticator (opens browser)
var result = await WebAuthenticator.AuthenticateAsync(
    new Uri(authUrlResult.Data), // https://identity.gov/oauth/authorize?...
    new Uri("credvault://oauth-callback")
);

// 3. Extract authorization code from callback
var authCode = result.Properties["code"];
var state = result.Properties["state"];

// 4. Exchange code for access token
var tokenResult = await authFlowService.HandleOAuthCallbackAsync(authCode, state);
```

**Current Issue:** 
- ❌ Just shows a loading screen with fake progress bar
- ❌ No actual browser redirect
- ❌ No OAuth flow

---

### **Step 3: Request Credential Offer** ⚠️ NEEDS IMPLEMENTATION
**What should happen:**

```csharp
// After authentication success, request credential details from issuer

var offerResult = await authFlowService.RequestCredentialIssuanceAsync(
    credentialType: "NationalID",
    issuerId: "gov-id-authority"
);

// offerResult.Data contains:
// - Claims that will be included (name, DOB, ID number, photo, etc.)
// - Issuer details
// - Expiration date
// - Schema ID
```

**Current Issue:**
- ❌ Uses hardcoded mock claims
- ❌ No actual API call to issuer

---

### **Step 4: Show Consent & Review Page**
**Page:** `ConsentReviewPage`
**Status:** ⚠️ PARTIAL (UI exists, but shows mock data)

```
User sees:
✓ Credential Details
  - Full Name: John Doe
  - Date of Birth: August 12, 1990
  - Photo: [thumbnail]
  - ID Number: 123-456-7890
  - Valid Until: 12/31/2030

[✓] I have reviewed the information above
[✓] I consent to this credential being issued

[Confirm Issuance]
```

**What needs fixing:**
- ✅ UI is good
- ❌ Claims should come from `authFlowService.RequestCredentialIssuanceAsync()` result
- ❌ Claims are currently hardcoded

---

### **Step 5: Issue & Store Credential** ⚠️ NEEDS IMPLEMENTATION
**What should happen:**

```csharp
// When user taps "Confirm Issuance"
var storeResult = await authFlowService.AcceptAndStoreCredentialAsync(
    offerDetails: credentialOffer,
    userConsented: true
);

if (storeResult.IsSuccess)
{
    // Credential is now in wallet!
    var credential = storeResult.Data;
    
    // Show confirmation page
    await _navigationService.NavigateToAsync("confirmation");
}
```

**Current Issue:**
- ❌ No actual credential issuance
- ❌ No storage in wallet
- ❌ Just navigates to confirmation page with fake success

---

## 🛠️ What Needs to Be Implemented

### **1. Platform WebAuthenticator Integration**

```csharp
// In MauiProgram.cs - Already using Microsoft.Maui.Authentication

// Usage in AddCredentialViewModel:
#if ANDROID || IOS
var authResult = await WebAuthenticator.Default.AuthenticateAsync(
    new WebAuthenticatorOptions
    {
        Url = new Uri(authorizationUrl),
        CallbackUrl = new Uri("credvault://oauth-callback"),
        PrefersEphemeralWebBrowserSession = true
    });
#else
// Windows/Desktop: Open default browser and listen for callback
#endif
```

### **2. Deep Link Handling (OAuth Callback)**

**AndroidManifest.xml:**
```xml
<activity android:name=".MainActivity">
    <intent-filter>
        <action android:name="android.intent.action.VIEW" />
        <category android:name="android.intent.category.DEFAULT" />
        <category android:name="android.intent.category.BROWSABLE" />
        <data android:scheme="credvault" android:host="oauth-callback" />
    </intent-filter>
</activity>
```

**Info.plist (iOS):**
```xml
<key>CFBundleURLTypes</key>
<array>
    <dict>
        <key>CFBundleURLSchemes</key>
        <array>
            <string>credvault</string>
        </array>
    </dict>
</array>
```

### **3. Update AddCredentialViewModel**

Replace mock authentication with real flow:

```csharp
private readonly AuthenticationFlowService _authFlowService;

[RelayCommand(CanExecute = nameof(CanProceedToAuthentication))]
private async Task ProceedToAuthenticationAsync()
{
    if (SelectedCredentialType == null) return;

    try
    {
        IsBusy = true;
        CurrentStep = 2;

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

        // 2. Launch browser for authentication
        var authResult = await WebAuthenticator.Default.AuthenticateAsync(
            new Uri(authUrlResult.Data!),
            new Uri("credvault://oauth-callback")
        );

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

        // 5. Show consent page with REAL claims
        CredentialOffer = offerResult.Data;
        await LoadCredentialClaimsAsync(offerResult.Data);
        await ProceedToConsentReviewAsync();
    }
    catch (TaskCanceledException)
    {
        // User cancelled the authentication
        await ShowAlertAsync("Cancelled", "Authentication was cancelled", "OK");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Authentication failed");
        await ShowAlertAsync("Error", "Failed to authenticate", "OK");
    }
    finally
    {
        IsBusy = false;
    }
}
```

### **4. Update ConfirmIssuanceCommand**

Replace mock issuance with real credential storage:

```csharp
[RelayCommand(CanExecute = nameof(CanConfirmIssuance))]
private async Task ConfirmIssuanceAsync()
{
    if (!HasReviewedClaims || !HasConsentedToIssuance || CredentialOffer == null)
        return;

    try
    {
        IsBusy = true;

        // Store credential in wallet (REAL)
        var result = await _authFlowService.AcceptAndStoreCredentialAsync(
            CredentialOffer,
            userConsented: true
        );

        if (result.IsSuccess)
        {
            NewCredentialId = result.Data?.Id;
            CredentialAddedSuccessfully = true;
            await _navigationService.NavigateToAsync("confirmation");
        }
        else
        {
            await ShowAlertAsync("Error", result.ErrorMessage, "OK");
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to confirm issuance");
        await ShowAlertAsync("Error", "Failed to add credential", "OK");
    }
    finally
    {
        IsBusy = false;
    }
}
```

---

## 📝 Summary

### **What We Have:**
✅ Beautiful UI pages matching Figma designs  
✅ Navigation flow between pages  
✅ ViewModels with MVVM structure  
✅ Mock data showing what it should look like  

### **What's Missing:**
❌ Real OAuth 2.0 authentication with browser redirect  
❌ OpenID4VCI credential offer/request protocol  
❌ WebAuthenticator integration  
❌ Deep link handling for OAuth callbacks  
❌ Token exchange and storage  
❌ Actual API calls to Identity Provider and Issuer  

### **Next Steps:**
1. ✅ Created `AuthenticationFlowService` with proper structure
2. ⏳ Register service in DI container
3. ⏳ Update `AddCredentialViewModel` to use `AuthenticationFlowService`
4. ⏳ Implement platform-specific OAuth handlers
5. ⏳ Test with real/mock Identity Provider
6. ⏳ Integrate with actual Issuer APIs

**The app is currently a "UI prototype" - it looks right but needs the authentication plumbing connected!** 🔌
