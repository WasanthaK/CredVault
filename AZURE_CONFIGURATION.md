# Azure Configuration - Production APIs

**Last Updated:** November 11, 2025  
**Status:** ✅ All services verified and documented

---

## 🌐 Azure API Management Gateway

**Base URL:** `https://apim-wallet-dev.azure-api.net`  
**Rate Limit:** 100 calls per 60 seconds  
**Region:** East US

### 🔑 Subscription Keys

**Display Name:** Raveen Enadoc - API Access  
**Subscription ID:** `raveen-subscription`  
**Status:** ✅ Active  
**Created:** November 3, 2025

| Type | Key |
|------|-----|
| **Primary** | `4a47f13f76d54eb999efc2036245ddc2` |
| **Secondary** | `e55561c2488642dca7922352ec886972` |

### 📡 Usage

All API requests must include the subscription key in the header:

```http
Ocp-Apim-Subscription-Key: 4a47f13f76d54eb999efc2036245ddc2
```

**PowerShell Example:**
```powershell
$headers = @{
    "Ocp-Apim-Subscription-Key" = "4a47f13f76d54eb999efc2036245ddc2"
}

Invoke-RestMethod -Uri "https://apim-wallet-dev.azure-api.net/wallet/health" `
    -Headers $headers
```

**C# Example (.NET MAUI):**
```csharp
var client = new HttpClient();
client.DefaultRequestHeaders.Add(
    "Ocp-Apim-Subscription-Key", 
    "4a47f13f76d54eb999efc2036245ddc2"
);

var response = await client.GetAsync(
    "https://apim-wallet-dev.azure-api.net/wallet/health"
);
```

---

## 1️⃣ Identity API (OAuth/OpenID Connect Provider)

### Service Details
| Property | Value |
|----------|-------|
| **Base Path** | `/identity` |
| **Container URL** | `http://wallet-identity.kindhill-eee6017a.eastus.azurecontainerapps.io` |
| **Swagger UI** | https://wallet-identity.kindhill-eee6017a.eastus.azurecontainerapps.io/api/v1/identity/swagger |
| **Swagger JSON** | https://wallet-identity.kindhill-eee6017a.eastus.azurecontainerapps.io/swagger/v1/swagger.json |

### OpenID Connect Discovery

✅ **Discovery Endpoint:**  
`/identity/.well-known/openid-configuration`

**Full URL:**  
https://wallet-identity.kindhill-eee6017a.eastus.azurecontainerapps.io/.well-known/openid-configuration

### OAuth 2.0 / OpenID Connect Endpoints

| Endpoint | URL |
|----------|-----|
| **Issuer** | `http://wallet-identity.kindhill-eee6017a.eastus.azurecontainerapps.io` |
| **Authorization** | `http://wallet-identity.kindhill-eee6017a.eastus.azurecontainerapps.io/connect/authorize` |
| **Token** | `http://wallet-identity.kindhill-eee6017a.eastus.azurecontainerapps.io/connect/token` |
| **UserInfo** | `http://wallet-identity.kindhill-eee6017a.eastus.azurecontainerapps.io/connect/userinfo` |
| **JWKS** | `http://wallet-identity.kindhill-eee6017a.eastus.azurecontainerapps.io/.well-known/jwks` |
| **Introspection** | `http://wallet-identity.kindhill-eee6017a.eastus.azurecontainerapps.io/connect/introspect` |
| **Revocation** | `http://wallet-identity.kindhill-eee6017a.eastus.azurecontainerapps.io/connect/revoke` |

### Supported Features

| Feature | Support |
|---------|---------|
| **Response Types** | `code` (Authorization Code Flow) |
| **PKCE Methods** | `S256` ✅ (SHA256) |
| **Scopes** | `openid`, `profile`, `email`, `roles` |
| **Token Auth Methods** | `client_secret_basic`, `client_secret_post` |
| **ID Token Signing** | RS256, RS384, RS512 |
| **Claims** | sub, name, email, role |

### OAuth Client Configuration

```csharp
// In ApiConfiguration.cs
public static class OAuth
{
    public const string AuthorizationEndpoint = 
        "http://wallet-identity.kindhill-eee6017a.eastus.azurecontainerapps.io/connect/authorize";
    public const string TokenEndpoint = 
        "http://wallet-identity.kindhill-eee6017a.eastus.azurecontainerapps.io/connect/token";
    public const string UserInfoEndpoint = 
        "http://wallet-identity.kindhill-eee6017a.eastus.azurecontainerapps.io/connect/userinfo";
    public const string Issuer = 
        "http://wallet-identity.kindhill-eee6017a.eastus.azurecontainerapps.io";
    
    public const string ClientId = "credvault-mobile";
    public const string RedirectUri = "credvault://oauth-callback";
    
    public static readonly string[] Scopes = 
        { "openid", "profile", "email", "roles" };
}
```

### PKCE Implementation (S256)

**Code Challenge Generation:**
```csharp
// 1. Generate code verifier (32 bytes random)
var codeVerifier = GenerateCodeVerifier(); 
// Example: "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk"

// 2. Generate SHA256 hash
using var sha256 = SHA256.Create();
var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));

// 3. Base64Url encode
var codeChallenge = Base64UrlEncode(challengeBytes);
// Example: "E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM"
```

**Authorization Request:**
```
GET /connect/authorize
  ?client_id=credvault-mobile
  &redirect_uri=credvault://oauth-callback
  &response_type=code
  &scope=openid profile email roles
  &code_challenge=E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM
  &code_challenge_method=S256
  &state={GUID}
```

**Token Exchange:**
```
POST /connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=authorization_code
&code={authorization_code}
&redirect_uri=credvault://oauth-callback
&client_id=credvault-mobile
&code_verifier={original_verifier}
```

**Response:**
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "refresh_token": "def50200b0e5c...",
  "id_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "scope": "openid profile email roles"
}
```

---

## 2️⃣ Wallet API (Credential Management)

### Service Details
| Property | Value |
|----------|-------|
| **Base Path** | `/wallet` |
| **Container URL** | `http://wallet-wallet.kindhill-eee6017a.eastus.azurecontainerapps.io` |
| **Swagger UI** | https://wallet-wallet.kindhill-eee6017a.eastus.azurecontainerapps.io/api/v1/wallet/swagger |
| **Swagger JSON** | https://wallet-wallet.kindhill-eee6017a.eastus.azurecontainerapps.io/swagger/v1/swagger.json |
| **Health Check** | `GET /api/v1/wallet/health` ✅ Healthy |

### Standards Supported
- ✅ W3C Verifiable Credentials
- ✅ OpenID4VCI (Verifiable Credential Issuance)
- ✅ OpenID4VP (Verifiable Presentation)
- ✅ ISO mDL (Mobile Driver's License)
- ✅ DIF Presentation Exchange

### Key Endpoint Categories

#### Authorization (`/api/v1/wallet/Authorization`)
- `POST /authorize` - Initiate OAuth flow
- `POST /token` - Exchange auth code for token
- `POST /validate` - Validate access token

#### Credential Discovery (`/api/v1/wallet/CredentialDiscovery`)
- `GET /credential_offer` - Get credential offer details

#### Credential Issuance (`/api/v1/wallet/CredentialIssuance`)
- `POST /credential` - Issue credential to holder
- `POST /batch_credential` - Issue multiple credentials

#### Credential Management (`/api/v1/wallet/Credential`)
- `POST /issue` - Issue new credential
- `GET /{credentialId}` - Get credential details
- `POST /{credentialId}/verify` - Verify credential validity
- `POST /{credentialId}/revoke` - Revoke credential
- `GET /holder/{holderId}` - Get holder's credentials
- `POST /{credentialId}/suspend` - Temporarily suspend credential
- `POST /{credentialId}/reactivate` - Reactivate suspended credential
- `GET /{credentialId}/status` - Get credential status
- `DELETE /{credentialId}` - Remove from wallet

#### Holder Operations (`/api/v1/wallet/Holder`)
- `GET /{id}` - Get holder details
- `POST /` - Create new holder
- `PUT /{id}` - Update holder
- `DELETE /{id}` - Delete holder
- `GET /credential/{credentialId}` - Get holder by credential
- `GET /present/{credentialId}` - Present credential

#### Issuer Operations (`/api/v1/wallet/Issuer`)
- `GET /` - List all issuers
- `GET /{id}` - Get issuer details
- `POST /register` - Register new issuer
- `POST /` - Create issuer
- `PUT /{id}` - Update issuer
- `DELETE /{id}` - Delete issuer
- `POST /issue` - Issue credential as issuer

#### Verifier Operations (`/api/v1/wallet/Verifier`)
- `GET /` - List all verifiers
- `GET /{id}` - Get verifier details
- `POST /register` - Register new verifier
- `POST /verify` - Verify credential
- `POST /validate-presentation` - Validate presentation

#### Device Transfer (`/api/v1/wallet/DeviceTransfer`)
- `POST /export` - Export credentials
- `POST /import` - Import credentials
- `POST /validate` - Validate transfer

### Response Format

All API responses use `ApiResponseDto<T>` wrapper:

```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": "Operation successful",
  "data": {
    // Actual response data
  },
  "errors": null
}
```

### Example: Request Credential Offer

```http
GET /api/v1/wallet/discovery/credential_offer?type=NationalID
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
      "nationalId": "123456789V"
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

## 3️⃣ Consent API

### Service Details
| Property | Value |
|----------|-------|
| **Base Path** | `/consent` |
| **Container URL** | `http://wallet-consent.kindhill-eee6017a.eastus.azurecontainerapps.io` |
| **Swagger UI** | https://wallet-consent.kindhill-eee6017a.eastus.azurecontainerapps.io/api/v1/consent/swagger |
| **Swagger JSON** | https://wallet-consent.kindhill-eee6017a.eastus.azurecontainerapps.io/swagger/v1/swagger.json |
| **Status** | ✅ Available |

### Purpose
Manages user consent for credential issuance and presentation. Tracks consent history and revocation.

---

## 4️⃣ GovStack Payments Building Block API

### Service Details
| Property | Value |
|----------|-------|
| **Base Path** | `/payments` |
| **Container URL** | `http://wallet-payments.kindhill-eee6017a.eastus.azurecontainerapps.io` |
| **Swagger UI** | https://wallet-payments.kindhill-eee6017a.eastus.azurecontainerapps.io/api/v1/payments/swagger |
| **Swagger JSON** | https://wallet-payments.kindhill-eee6017a.eastus.azurecontainerapps.io/swagger/v1/swagger.json |
| **Status** | ✅ Verified (43 endpoints) |

### Purpose
Handles payment processing for premium credentials and wallet features.

---

## 🧪 Test Accounts

### Test User (Holder)

| Property | Value |
|----------|-------|
| **Email** | wasanthak@enadoc.com |
| **Password** | Passw0rd! |
| **User ID** | b7745358-49ea-40a4-9ae7-aa81193eed5f |
| **Name** | Wasantha Karunathilaka |
| **Status** | ✅ Active |

### Test Issuers

| Issuer | ID | Type |
|--------|-----|------|
| Government ID Authority | `gov-id-authority` | National ID |
| Department of Motor Vehicles | `dmv` | Driver's License |
| State University | `state-university` | Diploma |

---

## 🔧 Implementation Status

### ✅ Completed (November 11, 2025)
1. **PKCE OAuth Flow** - Full implementation with S256 code challenge
2. **Deep Links** - Android & iOS configured for `credvault://oauth-callback`
3. **Models** - CredentialOfferDetails, IssuerMetadata, PKCEParameters
4. **API Configuration** - OAuth endpoints in ApiConfiguration.cs
5. **Token Storage** - SecureStorage implementation
6. **Azure Discovery** - All 4 services documented with Swagger URLs

### 🟡 In Progress
- ViewModel integration with OAuth service
- Credential offer retrieval
- Credential storage in wallet

### 🔴 Not Started
- Payment API integration
- Consent API integration

---

## 📚 Related Documentation

- **Implementation Guide:** See `IMPLEMENTATION_GUIDE.md` for step-by-step OAuth flow
- **Architecture:** See `ARCHITECTURE.md` for system design
- **API Mapping:** See `API_REFERENCE.md` for UI to endpoint mapping

---

## 🔒 Security Notes

1. **Never commit subscription keys** - Use environment variables or Azure Key Vault
2. **PKCE is mandatory** - S256 code challenge method only
3. **Tokens in SecureStorage** - Never in plain text or preferences
4. **HTTPS only** - All production endpoints must use HTTPS (containers use HTTP internally, APIM provides TLS termination)
5. **State parameter** - Always validate for CSRF protection
6. **Ephemeral browser sessions** - No persistent cookies for OAuth

---

**Last Verified:** November 11, 2025  
**Next Review:** When adding new services or endpoints
