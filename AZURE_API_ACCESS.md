# 🔐 Azure API Access for CredVault

**Last Updated**: November 3, 2025

---

## 📋 Overview

All microservices have been deployed to **Azure Container Apps** and are accessible through **Azure API Management (APIM)**.

---

## 🔑 API Subscription Details

### Raveen Enadoc (raveen@enadoc.com)

**Display Name:** Raveen Enadoc - API Access  
**Subscription ID:** `raveen-subscription`  
**Status:** ✅ Active  
**Created:** November 3, 2025

#### Primary Key
```
4a47f13f76d54eb999efc2036245ddc2
```

#### Secondary Key
```
e55561c2488642dca7922352ec886972
```

---

## 🚀 Usage

All API requests must include the subscription key in the header:

```bash
curl -H "Ocp-Apim-Subscription-Key: 4a47f13f76d54eb999efc2036245ddc2" \
  https://apim-wallet-dev.azure-api.net/wallet/api/health
```

### PowerShell Example
```powershell
$headers = @{
    "Ocp-Apim-Subscription-Key" = "4a47f13f76d54eb999efc2036245ddc2"
}

Invoke-RestMethod -Uri "https://apim-wallet-dev.azure-api.net/wallet/health" -Headers $headers
```

### C# Example (.NET MAUI)
```csharp
using System.Net.Http;

var client = new HttpClient();
client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "4a47f13f76d54eb999efc2036245ddc2");

var response = await client.GetAsync("https://apim-wallet-dev.azure-api.net/wallet/health");
```

---

## 🌐 API Gateway

**Base URL:** `https://apim-wallet-dev.azure-api.net`

All microservices are accessible through this gateway with their respective base paths.

---

## 📡 Available APIs

### 1. Wallet API ✅

**Base Path:** `/wallet`

| Endpoint | URL |
|----------|-----|
| **Health Check** | `https://apim-wallet-dev.azure-api.net/wallet/health` |
| **Swagger UI (Direct)** | `https://wallet-wallet.kindhill-eee6017a.eastus.azurecontainerapps.io/api/v1/wallet/swagger` |
| **Well-Known Config** | `https://apim-wallet-dev.azure-api.net/wallet/.well-known/openid-credential-issuer` |

#### Key Endpoints
- **Authorization**
  - `POST /wallet/api/v1/wallet/Authorization/authorize` - Authorization endpoint
  - `POST /wallet/api/v1/wallet/Authorization/token` - Get access token
  - `POST /wallet/api/v1/wallet/Authorization/validate` - Validate token

- **Credentials**
  - `POST /wallet/api/v1/wallet/Credential/issue` - Issue new credential
  - `GET /wallet/api/v1/wallet/Credential/{credentialId}` - Get credential details
  - `POST /wallet/api/v1/wallet/Credential/{credentialId}/verify` - Verify credential
  - `POST /wallet/api/v1/wallet/Credential/{credentialId}/revoke` - Revoke credential
  - `GET /wallet/api/v1/wallet/Credential/holder/{holderId}` - Get holder's credentials
  - `POST /wallet/api/v1/wallet/Credential/{credentialId}/suspend` - Suspend credential
  - `POST /wallet/api/v1/wallet/Credential/{credentialId}/reactivate` - Reactivate credential
  - `GET /wallet/api/v1/wallet/Credential/{credentialId}/status` - Get credential status

---

### 2. Identity API ✅

**Base Path:** `/identity`

| Endpoint | URL |
|----------|-----|
| **Health Check** | `https://apim-wallet-dev.azure-api.net/identity/health` |
| **Swagger UI (Direct)** | `https://wallet-identity.kindhill-eee6017a.eastus.azurecontainerapps.io/api/v1/identity/swagger` |
| **OpenID Config** | `https://apim-wallet-dev.azure-api.net/identity/.well-known/openid-configuration` |

#### Key Endpoints

- **Authentication**
  - `POST /identity/api/v1/identity/auth/register` - **Create new user account**
  - `POST /identity/connect/token` - Get access token
  - `GET /identity/connect/authorize` - Authorization endpoint

- **User Management**
  - `GET /identity/api/v1/identity/users` - List users
  - `GET /identity/api/v1/identity/users/{userId}` - Get user details
  - `POST /identity/api/v1/identity/users/change-password` - Change password

#### User Registration Request
```json
{
  "username": "string",
  "email": "user@example.com",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe",
  "ipAddress": "192.168.1.1",
  "userAgent": "CredVault Mobile/1.0"
}
```

**Response:** `201 Created` with user details

---

### 3. Consent API ✅

**Base Path:** `/consent`

| Endpoint | URL |
|----------|-----|
| **Health Check** | `https://apim-wallet-dev.azure-api.net/consent/health` |
| **Swagger UI (Direct)** | `https://wallet-consent.kindhill-eee6017a.eastus.azurecontainerapps.io/api/v1/consent/swagger` |

#### Key Endpoints
- `GET /consent/health` - Service health status
- `POST /consent/api/consents` - Create consent
- `POST /consent/api/v1/consent/consents` - Create consent (v1)
- `POST /consent/service/delegation` - Delegation operations

---

### 4. Payments API ⚠️

**Base Path:** `/payments`

| Endpoint | URL |
|----------|-----|
| **Health Check** | TBD (needs verification) |
| **Swagger UI (Direct)** | `https://wallet-payments.kindhill-eee6017a.eastus.azurecontainerapps.io/swagger` |

> ⚠️ **Note:** Payments API may have limited operations imported through APIM. For full access, use direct service URLs or contact support.

---

## 🚦 Rate Limits

- **100 calls per 60 seconds** per subscription key
- Rate limit headers included in responses:
  - `X-RateLimit-Limit`
  - `X-RateLimit-Remaining`
  - `X-RateLimit-Reset`

---

## 🔧 Testing APIs

### Test Wallet API Health
```bash
curl -H "Ocp-Apim-Subscription-Key: 4a47f13f76d54eb999efc2036245ddc2" \
  https://apim-wallet-dev.azure-api.net/wallet/health
```

### Test Identity API Health
```bash
curl -H "Ocp-Apim-Subscription-Key: 4a47f13f76d54eb999efc2036245ddc2" \
  https://apim-wallet-dev.azure-api.net/identity/health
```

### Test User Registration
```bash
curl -X POST \
  -H "Ocp-Apim-Subscription-Key: 4a47f13f76d54eb999efc2036245ddc2" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "Test123!",
    "confirmPassword": "Test123!",
    "firstName": "Test",
    "lastName": "User",
    "ipAddress": "127.0.0.1",
    "userAgent": "curl"
  }' \
  https://apim-wallet-dev.azure-api.net/identity/api/v1/identity/auth/register
```

---

## 🏗️ Direct Service URLs (Bypass APIM)

For debugging or when APIM is unavailable:

| Service | Direct URL |
|---------|------------|
| **Wallet** | `https://wallet-wallet.kindhill-eee6017a.eastus.azurecontainerapps.io` |
| **Identity** | `https://wallet-identity.kindhill-eee6017a.eastus.azurecontainerapps.io` |
| **Consent** | `https://wallet-consent.kindhill-eee6017a.eastus.azurecontainerapps.io` |
| **Payments** | `https://wallet-payments.kindhill-eee6017a.eastus.azurecontainerapps.io` |

> ⚠️ **Note:** Direct URLs bypass APIM rate limiting and subscription key validation. Use APIM gateway for production.

---

## 📝 Implementation in Mobile App

### Update ApiConfiguration.cs

```csharp
public static class ApiConfiguration
{
    // Azure APIM Gateway
    public const string BaseUrl = "https://apim-wallet-dev.azure-api.net";
    
    // API Keys
    public const string SubscriptionKey = "4a47f13f76d54eb999efc2036245ddc2";
    
    // Service Base Paths
    public const string WalletBasePath = "/wallet/api/v1/wallet";
    public const string IdentityBasePath = "/identity/api/v1/identity";
    public const string ConsentBasePath = "/consent/api/v1/consent";
    public const string PaymentsBasePath = "/payments/api/v1/payments";
    
    // Full URLs
    public static string WalletApiUrl => $"{BaseUrl}{WalletBasePath}";
    public static string IdentityApiUrl => $"{BaseUrl}{IdentityBasePath}";
    public static string ConsentApiUrl => $"{BaseUrl}{ConsentBasePath}";
    public static string PaymentsApiUrl => $"{BaseUrl}{PaymentsBasePath}";
}
```

### Update HttpClient Configuration

```csharp
// In MauiProgram.cs or your DI setup
builder.Services.AddHttpClient("WalletApi", client =>
{
    client.BaseAddress = new Uri(ApiConfiguration.WalletApiUrl);
    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ApiConfiguration.SubscriptionKey);
});

builder.Services.AddHttpClient("IdentityApi", client =>
{
    client.BaseAddress = new Uri(ApiConfiguration.IdentityApiUrl);
    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ApiConfiguration.SubscriptionKey);
});
```

---

## 🆘 Troubleshooting

### Issue: 401 Unauthorized
**Solution:** Verify subscription key is included in request header:
```
Ocp-Apim-Subscription-Key: 4a47f13f76d54eb999efc2036245ddc2
```

### Issue: 429 Too Many Requests
**Solution:** Rate limit exceeded (100 calls/60s). Implement retry logic with exponential backoff.

### Issue: 404 Not Found
**Solution:** Check the base path is correct (e.g., `/wallet`, `/identity`, etc.)

### Issue: 500 Internal Server Error
**Solution:** Check Azure Container App logs or try direct service URL for more details.

---

## 📞 Support

For API access issues or additional subscription keys, contact:
- **Email:** raveen@enadoc.com
- **Azure Portal:** Check APIM subscriptions and service health

---

## 🔄 Migration from Docker to Azure

| Environment | Base URL | Auth Method |
|-------------|----------|-------------|
| **Local Docker** | `http://localhost:7015` | None |
| **Azure APIM** | `https://apim-wallet-dev.azure-api.net` | Subscription Key Required |

### Key Differences
1. **Subscription Key Required:** All Azure requests need `Ocp-Apim-Subscription-Key` header
2. **Rate Limiting:** 100 calls per 60 seconds enforced
3. **Path Changes:** Services have base paths (`/wallet`, `/identity`, etc.)
4. **HTTPS Only:** All connections must use HTTPS

---

## ✅ Next Steps

1. ✅ Update `ApiConfiguration.cs` with Azure URLs
2. ✅ Add subscription key to HTTP client
3. ✅ Update all API service classes
4. ✅ Test registration and login flows
5. ✅ Update environment configuration for dev/prod switching
