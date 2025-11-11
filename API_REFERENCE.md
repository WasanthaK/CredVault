# API Reference - UI to Endpoint Mapping

**Last Updated:** November 11, 2025  
**Production Base URL:** `https://apim-wallet-dev.azure-api.net`

> **See Also:**  
> - `AZURE_CONFIGURATION.md` - Complete Azure service configuration  
> - `IMPLEMENTATION_GUIDE.md` - OAuth flow implementation details

---

## 🔑 Authentication

**Required Headers:**
```http
Ocp-Apim-Subscription-Key: 4a47f13f76d54eb999efc2036245ddc2
Authorization: Bearer {access_token}  (for protected endpoints)
```

**Rate Limit:** 100 calls per 60 seconds

---

## 📱 Dashboard / Home Screen

**UI File:** `dashboard_/_home/code.html`  
**ViewModel:** `DashboardViewModel.cs`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| **Credential Carousel** | `GET /wallet/api/v1/wallet/Credential/holder/{holderId}` | GET | Load all user credentials |
| **Credential Status Badge** | `GET /wallet/api/v1/wallet/Credential/{credentialId}/status` | GET | Check if credential is active/suspended/revoked |
| **Activity Feed** | `GET /wallet/api/v1/wallet/Wallet/logs` | GET | Display recent credential activity |
| **User Profile** | `GET /identity/connect/userinfo` | GET | Get user details (name, email, photo) |

**Example Request:**
```http
GET /wallet/api/v1/wallet/Credential/holder/b7745358-49ea-40a4-9ae7-aa81193eed5f
Ocp-Apim-Subscription-Key: 4a47f13f76d54eb999efc2036245ddc2
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": [
    {
      "credentialId": "cred-123",
      "credentialType": "NationalID",
      "issuer": { "name": "Government ID Authority" },
      "status": "Active",
      "issuedDate": "2025-11-11T10:00:00Z",
      "expiryDate": "2030-12-31T23:59:59Z"
    }
  ]
}
```

---

## ➕ Add Credential Flow

### 1. Select Credential Type

**UI File:** `add_credential_-_select_type_1/code.html`  
**ViewModel:** `AddCredentialViewModel.cs`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| **Credential Type Grid** | `GET /wallet/api/v1/wallet/Issuer` | GET | List available issuers and credential types |
| **"Continue" Button** | *(Navigation only)* | - | Proceed to authentication |

**Example Request:**
```http
GET /wallet/api/v1/wallet/Issuer
Ocp-Apim-Subscription-Key: 4a47f13f76d54eb999efc2036245ddc2
```

**Response:**
```json
{
  "isSuccess": true,
  "data": [
    {
      "id": "gov-id-authority",
      "name": "Government ID Authority",
      "credentialTypes": ["NationalID"],
      "logoUrl": "https://...",
      "description": "Official government identity credentials"
    },
    {
      "id": "dmv",
      "name": "Department of Motor Vehicles",
      "credentialTypes": ["DriversLicense"],
      "logoUrl": "https://...",
      "description": "State-issued driver's licenses"
    }
  ]
}
```

---

### 2. Authenticate with Issuer

**UI Files:** `add_credential_-_authenticate_1/code.html`, `add_credential_-_authenticate_2/code.html`  
**Service:** `AuthenticationFlowService.cs`

#### Step 2a: Start OAuth Flow

| Action | Implementation | Details |
|--------|----------------|---------|
| **Generate PKCE** | `AuthenticationFlowService.GeneratePKCEParameters()` | Creates 32-byte verifier + SHA256 challenge |
| **Build Auth URL** | `AuthenticationFlowService.BuildAuthorizationUrlWithPKCE()` | Constructs OAuth URL |
| **Open Browser** | `WebAuthenticator.AuthenticateAsync()` | Launches platform browser |

**Authorization URL:**
```
GET /connect/authorize
  ?client_id=credvault-mobile
  &redirect_uri=credvault://oauth-callback
  &response_type=code
  &scope=openid profile email roles
  &code_challenge={SHA256_base64url}
  &code_challenge_method=S256
  &state={GUID}
```

#### Step 2b: Handle Callback

**Deep Link:** `credvault://oauth-callback?code={auth_code}&state={state}`

| Action | Implementation | Details |
|--------|----------------|---------|
| **Validate State** | Compare with stored state | CSRF protection |
| **Extract Code** | Parse query parameters | Authorization code |
| **Exchange for Token** | `POST /connect/token` | Get access token |

**Token Exchange Request:**
```http
POST /connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=authorization_code
&code={authorization_code}
&redirect_uri=credvault://oauth-callback
&client_id=credvault-mobile
&code_verifier={original_32byte_verifier}
```

**Token Response:**
```json
{
  "access_token": "eyJhbGc...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "refresh_token": "def502...",
  "id_token": "eyJhbGc...",
  "scope": "openid profile email roles"
}
```

---

### 3. Request Credential Offer

**Service:** `AuthenticationFlowService.RequestCredentialIssuanceAsync()`

**Endpoint:**
```http
GET /wallet/api/v1/wallet/discovery/credential_offer?type=NationalID
Authorization: Bearer {access_token}
Ocp-Apim-Subscription-Key: 4a47f13f76d54eb999efc2036245ddc2
```

**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "credentialType": "NationalID",
    "format": "jwt_vc_json",
    "credentialSubject": {
      "firstName": "Wasantha",
      "lastName": "Karunathilaka",
      "dateOfBirth": "1990-01-01",
      "nationalId": "123456789V",
      "address": "Colombo, Sri Lanka",
      "photo": "base64..."
    },
    "issuer": {
      "id": "gov-id-authority",
      "name": "Government ID Authority",
      "credentialEndpoint": "https://..."
    },
    "credentialSchema": "https://schema.gov.lk/national-id/v1",
    "expirationDate": "2030-12-31T23:59:59Z"
  }
}
```

---

### 4. Review & Consent

**UI Files:** `add_credential_-_consent_&_review/code.html`, `add_credential_-_review_&_consent/code.html`  
**Page:** `ConsentReviewPage.xaml`

| UI Element | Data Source | Purpose |
|------------|-------------|---------|
| **Credential Type Label** | `CredentialOffer.CredentialType` | Display type (e.g., "National ID") |
| **Issuer Name** | `CredentialOffer.Issuer.Name` | Display issuer |
| **Claims List** | `CredentialOffer.Claims` | Show all claims to be stored |
| **Expiry Date** | `CredentialOffer.ExpiresAt` | Show when credential expires |
| **Consent Checkboxes** | Local UI state | User agreement |

**User Actions:**
- ☑ I have reviewed the information
- ☑ I consent to store this credential
- Tap "Confirm Issuance"

---

### 5. Issue & Store Credential

**Service:** `AuthenticationFlowService.AcceptAndStoreCredentialAsync()`

**Endpoint:**
```http
POST /wallet/api/v1/wallet/credential
Authorization: Bearer {access_token}
Ocp-Apim-Subscription-Key: 4a47f13f76d54eb999efc2036245ddc2
Content-Type: application/json
```

**Request Body:**
```json
{
  "format": "jwt_vc_json",
  "credentialDefinition": {
    "type": ["VerifiableCredential", "NationalID"]
  },
  "proof": {
    "proofType": "jwt",
    "jwt": "{access_token}"
  }
}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "credential": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
    "format": "jwt_vc_json",
    "c_nonce": "8b7f2e...",
    "c_nonce_expires_in": 86400,
    "transactionId": "txn-abc123"
  }
}
```

**Storage:**
- Store credential in `SecureStorage` with key `credential_{guid}`
- Add credential ID to `credentials_list` array
- Update dashboard to refresh credentials

---

### 6. Confirmation Screen

**UI Files:** `add_credential_-_confirmation_1/code.html`, `add_credential_-_confirmation_2/code.html`  
**Page:** `CredentialConfirmationPage.xaml`

| UI Element | Action | Purpose |
|------------|--------|---------|
| **Success Icon** | Display checkmark | Visual confirmation |
| **"View Credential"** | Navigate to credential details | Show full credential |
| **"Back to Wallet"** | Navigate to dashboard | Return to home |

---

## 📄 Credential Details Screen

**UI Files:** `credential_details_1/code.html`, `credential_details_2/code.html`  
**Page:** `CredentialDetailsPage.xaml`  
**ViewModel:** `CredentialDetailsViewModel.cs`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| **Load Details** | `GET /wallet/api/v1/wallet/Credential/{credentialId}` | GET | Get full credential data |
| **Status Badge** | `GET /wallet/api/v1/wallet/Credential/{credentialId}/status` | GET | Show current status (Active/Suspended/Revoked) |
| **"Verify Status" Button** | `POST /wallet/api/v1/wallet/Credential/{credentialId}/verify` | POST | Verify credential is still valid |
| **"Suspend" Button** | `POST /wallet/api/v1/wallet/Credential/{credentialId}/suspend` | POST | Temporarily suspend credential |
| **"Reactivate" Button** | `POST /wallet/api/v1/wallet/Credential/{credentialId}/reactivate` | POST | Reactivate suspended credential |
| **"Revoke" Button** | `POST /wallet/api/v1/wallet/Credential/{credentialId}/revoke` | POST | Permanently revoke credential |
| **"Delete" Button** | `DELETE /wallet/api/v1/wallet/Credential/{credentialId}` | DELETE | Remove from wallet |

**Example - Get Credential Details:**
```http
GET /wallet/api/v1/wallet/Credential/cred-123
Authorization: Bearer {access_token}
Ocp-Apim-Subscription-Key: 4a47f13f76d54eb999efc2036245ddc2
```

**Example - Suspend Credential:**
```http
POST /wallet/api/v1/wallet/Credential/cred-123/suspend
Authorization: Bearer {access_token}
Ocp-Apim-Subscription-Key: 4a47f13f76d54eb999efc2036245ddc2
Content-Type: application/json

{
  "reason": "Lost phone, temporary suspension"
}
```

---

## 🔍 Selective Disclosure (Presentation)

**UI Files:** `selective_disclosure_-_request_1/code.html`, `selective_disclosure_-_preview/code.html`  
**Service:** `PresentationService.cs`

### Presentation Request

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| **QR Code Scanner** | *(Local)* | - | Scan verifier QR code |
| **Parse Request** | `POST /wallet/api/v1/wallet/Verifier/validate-presentation` | POST | Validate presentation definition |
| **Display Claims** | *(Local from credential)* | - | Show requested vs. available claims |
| **Select Claims** | *(Local UI)* | - | User chooses which claims to share |

### Create Presentation

**Endpoint:**
```http
POST /wallet/api/v1/wallet/Holder/present/{credentialId}
Authorization: Bearer {access_token}
Ocp-Apim-Subscription-Key: 4a47f13f76d54eb999efc2036245ddc2
Content-Type: application/json
```

**Request Body:**
```json
{
  "presentationDefinition": {
    "id": "pres-def-123",
    "input_descriptors": [
      {
        "id": "national_id",
        "constraints": {
          "fields": [
            { "path": ["$.credentialSubject.firstName"] },
            { "path": ["$.credentialSubject.dateOfBirth"] }
          ]
        }
      }
    ]
  },
  "selectedClaims": ["firstName", "dateOfBirth"]
}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "presentation": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
    "format": "jwt_vp",
    "presentationSubmission": {
      "id": "sub-123",
      "definition_id": "pres-def-123"
    }
  }
}
```

---

## ⚙️ Settings & Profile

**UI Files:** `settings_-_profile_&_backup/code.html`, `settings_-_security_1/code.html`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| **User Profile** | `GET /identity/connect/userinfo` | GET | Load user details |
| **Update Profile** | `PUT /identity/api/v1/User/{userId}` | PUT | Update user info |
| **Export Credentials** | `POST /wallet/api/v1/wallet/DeviceTransfer/export` | POST | Backup credentials |
| **Import Credentials** | `POST /wallet/api/v1/wallet/DeviceTransfer/import` | POST | Restore credentials |

---

## 🔐 Status Management

**UI File:** `status_management/code.html`

| Action | API Endpoint | Method | Request Body |
|--------|--------------|--------|--------------|
| **Suspend** | `POST /wallet/api/v1/wallet/Credential/{id}/suspend` | POST | `{ "reason": "..." }` |
| **Reactivate** | `POST /wallet/api/v1/wallet/Credential/{id}/reactivate` | POST | `{}` |
| **Revoke** | `POST /wallet/api/v1/wallet/Credential/{id}/revoke` | POST | `{ "reason": "..." }` |
| **Check Status** | `GET /wallet/api/v1/wallet/Credential/{id}/status` | GET | - |

**Status Values:**
- `Active` - Credential is valid and can be used
- `Suspended` - Temporarily disabled (can be reactivated)
- `Revoked` - Permanently disabled (cannot be reactivated)
- `Expired` - Past expiration date

---

## 🔍 Verifier Mode

**UI Files:** `verifier_mode_-_scan_home/code.html`, `verifier_mode_-_scan_result_1/code.html`

| Action | API Endpoint | Method | Purpose |
|--------|--------------|--------|---------|
| **Register Verifier** | `POST /wallet/api/v1/wallet/Verifier/register` | POST | Register as verifier |
| **Create Request** | `POST /wallet/api/v1/wallet/Verifier/create-request` | POST | Generate QR code for presentation request |
| **Validate Presentation** | `POST /wallet/api/v1/wallet/Verifier/validate-presentation` | POST | Verify presented credential |

---

## 📊 Response Format

All Wallet API responses use `ApiResponseDto<T>` wrapper:

```csharp
public class ApiResponseDto<T>
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
    public List<string> Errors { get; set; }
}
```

**Success Response:**
```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": "Operation successful",
  "data": { /* actual data */ },
  "errors": null
}
```

**Error Response:**
```json
{
  "isSuccess": false,
  "statusCode": 400,
  "message": "Validation failed",
  "data": null,
  "errors": [
    "Credential type is required",
    "Invalid issuer ID"
  ]
}
```

---

## 🧪 Testing Endpoints

**Test Account:**
- Email: wasanthak@enadoc.com
- Password: Passw0rd!
- User ID: b7745358-49ea-40a4-9ae7-aa81193eed5f

**Quick Health Check:**
```powershell
# Test Wallet API
Invoke-RestMethod -Uri "https://apim-wallet-dev.azure-api.net/wallet/health" `
    -Headers @{"Ocp-Apim-Subscription-Key" = "4a47f13f76d54eb999efc2036245ddc2"}

# Test Identity API (OpenID Config)
Invoke-RestMethod -Uri "https://wallet-identity.kindhill-eee6017a.eastus.azurecontainerapps.io/.well-known/openid-configuration"
```

---

## 📚 Related Documentation

- **Azure Configuration:** `AZURE_CONFIGURATION.md` - Service details, OAuth config, test accounts
- **Implementation Guide:** `IMPLEMENTATION_GUIDE.md` - Step-by-step OAuth and credential flow
- **Architecture:** `ARCHITECTURE.md` - System design and patterns

---

**Last Updated:** November 11, 2025  
**API Version:** v1  
**Next Review:** When new endpoints are added
