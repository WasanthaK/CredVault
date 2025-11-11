# 🏗️ GovStack Digital Wallet - Architecture Overview

**Last Updated**: November 11, 2025

> **Recent Updates (Nov 11, 2025)**:
> - ✅ OAuth/PKCE implementation complete with Azure Identity API
> - ✅ Deep links configured for Android & iOS
> - ✅ All 4 Azure services documented with Swagger URLs
> - See `AZURE_SERVICES_CONFIGURATION.md` for complete service details

---

## System Architecture

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                          MOBILE APPLICATION                                  │
│                            (.NET MAUI 8)                                     │
│                                                                              │
│  ┌──────────────┐  ┌──────────────┐  ┌────────────────────────────────┐   │
│  │     Views    │  │  ViewModels  │  │         Services               │   │
│  │    (XAML)    │◄─┤    (MVVM)    │◄─┤  API Clients + Cache + Auth    │   │
│  └──────────────┘  └──────────────┘  └────────────────────────────────┘   │
│                                             │                               │
└─────────────────────────────────────────────┼───────────────────────────────┘
                                              │
        ┌─────────────────────────────────────┼─────────────────────────────┐
        │                │                    │              │               │
        ▼                ▼                    ▼              ▼               ▼
 ┌────────────┐   ┌────────────┐   ┌─────────────┐   ┌────────────┐
 │   WALLET   │   │  IDENTITY  │   │   CONSENT   │   │  PAYMENTS  │
 │   :7015    │   │   :7001    │   │   :7002     │   │   :7004    │
 │   🐳       │   │   🐳       │   │   🐳        │   │   🐳       │
 │            │   │            │   │             │   │            │
 │Credentials │   │   Auth     │   │User Consent │   │  Payment   │
 │OpenID4VCI  │   │OAuth/OIDC  │   │Permissions  │   │Processing  │
 │Verify      │   │  Users     │   │Data Sharing │   │            │
 └─────┬──────┘   └─────┬──────┘   └──────┬──────┘   └─────┬──────┘
       │                │                  │                │
       └────────────────┼──────────────────┼────────────────┘
                        │                  │
       ┌────────────────┼──────────────────┼───────────────┐
       │                │                  │               │
       ▼                ▼                  ▼               ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│  PostgreSQL  │  │  SQL Server  │  │    Redis     │
│   :5432      │  │   :1433      │  │   :6379      │
│   🐳         │  │   🐳         │  │   🐳         │
│              │  │              │  │              │
│ postgres:15  │  │ mssql:2022   │  │  redis:7     │
└──────────────┘  └──────────────┘  └──────────────┘
```

**Docker Compose Stack**: `mciroservices-*` containers (7 total)

---

## Mobile App Architecture

### Technology Stack

| Component | Technology | Purpose |
|-----------|------------|---------|
| **Framework** | .NET MAUI 8 | Cross-platform mobile (iOS/Android) |
| **Pattern** | MVVM | Separation of concerns, testability |
| **DI** | Microsoft.Extensions.DependencyInjection | Dependency injection |
| **HTTP** | RestSharp / Refit | API communication |
| **Storage** | SecureStorage | Token & sensitive data storage |
| **Biometrics** | MAUI Essentials | Face ID / Touch ID / Fingerprint |
| **QR Codes** | ZXing.Net.Maui | QR scanning & generation |
| **UI Toolkit** | CommunityToolkit.Maui | Advanced UI components |
| **Navigation** | Shell Navigation | App navigation & routing |

---

### API Endpoints Mapping

### Wallet API

**Azure (Production):** `https://apim-wallet-dev.azure-api.net/wallet/api/v1/wallet`  
**Local (Docker):** `http://localhost:7015/api/v1`

> ⚠️ **Note:** All Azure requests require `Ocp-Apim-Subscription-Key` header

#### Authorization
- `GET /Authorization/authorize` - Initiate OAuth2 flow
- `POST /Authorization/token` - Exchange code for access token
- `POST /Authorization/validate` - Validate existing token

#### Credentials (Holder Operations)
- `POST /Credential/issue` - Issue new credential
- `GET /Credential/{credentialId}` - Get credential details
- `POST /Credential/{credentialId}/verify` - Verify credential validity
- `POST /Credential/{credentialId}/revoke` - Revoke credential
- `GET /Credential/holder/{holderId}` - List all holder's credentials
- `POST /Credential/{credentialId}/suspend` - Suspend credential
- `POST /Credential/{credentialId}/reactivate` - Reactivate suspended credential
- `GET /Credential/{credentialId}/status` - Check credential status
- `DELETE /Credential/{credentialId}` - Delete credential
- `POST /Credential/query` - Query credentials with filters

#### Credential Discovery & Issuance (OpenID4VCI)
- `GET /CredentialDiscovery/credential_offer` - Get credential offer
- `POST /CredentialIssuance/credential` - Request credential issuance
- `POST /CredentialIssuance/batch_credential` - Batch credential issuance

#### Holder Management
- `GET /Holder/{id}` - Get holder profile
- `POST /Holder` - Create new holder
- `PUT /Holder/{id}` - Update holder profile
- `DELETE /Holder/{id}` - Delete holder
- `GET /Holder/credential/{credentialId}` - Get credential with holder info
- `GET /Holder/present/{credentialId}` - Present credential

#### Issuer Management
- `GET /Issuer` - List all issuers
- `GET /Issuer/{id}` - Get issuer details
- `POST /Issuer/register` - Register new issuer
- `POST /Issuer/issue` - Issue credential as issuer

#### Verification (Verifier Operations)
- `POST /Verifier/verify` - Verify credential
- `POST /Verifier/verify-presentation` - Verify presentation
- `POST /Verifier/verify/authorize` - Create presentation request
- `POST /Verifier/verify/vp-token` - Verify VP token

#### Device Transfer (Backup/Restore)
- `POST /DeviceTransfer/export` - Export wallet data
- `POST /DeviceTransfer/import` - Import wallet data
- `POST /DeviceTransfer/validate` - Validate backup package

#### Wallet Logs (Activity/Audit)
- `GET /Wallet/logs` - Get wallet activity logs
- `GET /Wallet/logs/count` - Get log count
- `GET /Wallet/credentials/{credentialId}/logs` - Get credential-specific logs

#### Workflow Orchestration
- `POST /Workflow/issue` - Workflow-based credential issuance
- `GET /Workflow/credentials/{credentialId}` - Get workflow status
- `GET /Workflow/citizens/{citizenSub}/credentials` - Get citizen credentials

---

### Identity API

**Azure (Production):** `https://apim-wallet-dev.azure-api.net/identity/api/v1/identity`  
**Local (Docker):** `http://localhost:7001`

#### Key Endpoints
- **Authentication**
  - `POST /auth/register` - Create new user account
  - `POST /connect/token` - Get access token
  - `GET /connect/authorize` - Authorization endpoint

- **User Management**
  - `GET /users` - List users
  - `GET /users/{userId}` - Get user details
  - `POST /users/change-password` - Change password

- **Configuration**
  - `GET /.well-known/openid-configuration` - OpenID configuration

---

### Consent API

**Azure (Production):** `https://apim-wallet-dev.azure-api.net/consent/api/v1/consent`  
**Local (Docker):** `http://localhost:7002`

#### Key Endpoints
- `POST /consents` - Create consent
- `POST /service/delegation` - Delegation operations

---

### Payments API

**Azure (Production):** `https://apim-wallet-dev.azure-api.net/payments/api/v1/payments`  
**Local (Docker):** `http://localhost:7004`

> ⚠️ **Note:** Limited operations available through APIM

---

## Data Flow Examples

### 1. User Login Flow

```
Mobile App                Identity Service          Wallet Service
    │                           │                        │
    ├─── POST /login ───────────►                       │
    │    (username, password)    │                       │
    │                           │                        │
    │◄─── access_token ──────────┤                       │
    │     refresh_token          │                       │
    │                           │                        │
    ├─── Store in SecureStorage                          │
    │                           │                        │
    ├─── GET /Holder/{userId} ─────────────────────────►│
    │    (Authorization: Bearer token)                   │
    │                           │                        │
    │◄─── Holder profile ────────────────────────────────┤
    │                           │                        │
    └─── Navigate to Dashboard  │                        │
```

### 2. Credential Issuance Flow (OpenID4VCI)

```
Mobile App                Wallet Service
    │                           │
    ├─── GET /credential_offer ►│
    │                           │
    │◄─── CredentialOffer ───────┤
    │     (issuer, types, grants)│
    │                           │
    ├─── GET /authorize ────────►│
    │    (offer details)         │
    │                           │
    │◄─── authorization_code ────┤
    │                           │
    ├─── POST /token ───────────►│
    │    (code, code_verifier)   │
    │                           │
    │◄─── access_token ──────────┤
    │                           │
    ├─── POST /credential ──────►│
    │    (type, access_token)    │
    │                           │
    │◄─── Credential ────────────┤
    │     (VC-JWT or SD-JWT)     │
    │                           │
    └─── Display "Success"      │
         Store credential        │
```

### 3. Credential Verification Flow (Verifier Mode)

```
Mobile App (Verifier)    Wallet Service         Mobile App (Holder)
    │                           │                        │
    ├─── POST /verify/authorize►│                       │
    │    (presentation_definition)                       │
    │                           │                        │
    │◄─── QR Code ───────────────┤                       │
    │     (request_id, challenge)│                       │
    │                           │                        │
    │                           │◄─── Scan QR Code ──────┤
    │                           │                        │
    │                           │◄─── POST /presentations│
    │                           │     (vp_token, proofs) │
    │                           │                        │
    │                           ├─── Verify Signature ───►
    │                           │    Validate Claims     │
    │                           │                        │
    │◄─── VerificationResult ────┤                       │
    │     (valid, claims)        │                        │
    │                           │                        │
    └─── Display Result        └────────────────────────┘
```

### 4. Device Transfer (Backup/Restore)

```
Old Device                Wallet Service          New Device
    │                           │                        │
    ├─── POST /DeviceTransfer/export                     │
    │    (holderId, credentials) │                        │
    │                           │                        │
    │◄─── Encrypted QR Data ─────┤                       │
    │                           │                        │
    │─── Display QR Code        │                        │
    │                           │                        │
    │                           │◄─── Scan QR ───────────┤
    │                           │                        │
    │                           │◄─── POST /import ──────┤
    │                           │     (encrypted data)   │
    │                           │                        │
    │                           ├─── Validate & Decrypt ─►
    │                           │                        │
    │                           ├─── Restore Credentials►│
    │                           │                        │
    │                           └────────────────────────►
```

---

## Security Considerations

### Authentication
- **JWT Bearer Tokens** for API authentication
- **OAuth2 + PKCE** for authorization flows
- **Refresh Tokens** for long-lived sessions
- **SecureStorage** for token persistence (iOS Keychain / Android KeyStore)

### Biometric Authentication
- **Platform APIs**: Face ID (iOS), Touch ID (iOS), Fingerprint (Android)
- **Fallback**: PIN code when biometrics unavailable
- **Use Cases**: App unlock, credential sharing, credential deletion

### Data Protection
- **In-Transit**: HTTPS/TLS 1.3 for all API calls
- **At-Rest**: Platform secure storage for sensitive data
- **Credentials**: Never stored in plain text, cached encrypted
- **Backup Data**: End-to-end encrypted before QR generation

### API Security
- **Rate Limiting**: Respect API rate limits
- **Token Expiry**: Automatic refresh before expiration
- **CORS**: Mobile app must be whitelisted
- **Certificate Pinning**: (Optional) Pin backend certificates

---

## Offline Capability

### Cached Data
- **Credentials**: Local encrypted cache for offline viewing
- **Activity Logs**: Sync when online
- **Settings**: Stored locally

### Sync Strategy
- **On App Launch**: Sync credentials and logs
- **Background Sync**: Periodic refresh
- **Manual Sync**: Pull-to-refresh gesture

### Offline Operations
- ✅ View credentials
- ✅ Generate QR codes for sharing
- ❌ Issue new credentials (requires network)
- ❌ Verify credentials (requires network)
- ❌ Backup/restore (requires network)

---

## Error Handling

### Network Errors
- Retry with exponential backoff
- Display user-friendly messages
- Cache requests for later retry

### API Errors
| Status Code | Handling |
|-------------|----------|
| 400 Bad Request | Display validation errors |
| 401 Unauthorized | Refresh token or re-login |
| 403 Forbidden | Show permission error |
| 404 Not Found | Resource doesn't exist |
| 429 Too Many Requests | Wait and retry |
| 500 Server Error | Generic error message, log details |

---

## Performance Targets

| Metric | Target |
|--------|--------|
| App Launch Time | < 2 seconds |
| API Response Time | < 500ms |
| Credential List Load | < 1 second |
| QR Code Generation | < 200ms |
| Credential Verification | < 2 seconds |
| Offline Mode Transition | < 100ms |

---

## Development Environments

### 🌐 Azure Production (Current)

**All microservices are deployed to Azure Container Apps**

| Service | URL | Status |
|---------|-----|--------|
| **API Gateway** | `https://apim-wallet-dev.azure-api.net` | ✅ Active |
| **Wallet API** | `https://apim-wallet-dev.azure-api.net/wallet` | ✅ Active |
| **Identity API** | `https://apim-wallet-dev.azure-api.net/identity` | ✅ Active |
| **Consent API** | `https://apim-wallet-dev.azure-api.net/consent` | ✅ Active |
| **Payments API** | `https://apim-wallet-dev.azure-api.net/payments` | ⚠️ Limited |

**Authentication:**
- **Subscription Key Required:** `Ocp-Apim-Subscription-Key: 4a47f13f76d54eb999efc2036245ddc2`
- **Rate Limit:** 100 calls per 60 seconds
- **Protocol:** HTTPS only

**Direct Container URLs (Bypass APIM):**
- Wallet: `https://wallet-wallet.kindhill-eee6017a.eastus.azurecontainerapps.io`
- Identity: `https://wallet-identity.kindhill-eee6017a.eastus.azurecontainerapps.io`
- Consent: `https://wallet-consent.kindhill-eee6017a.eastus.azurecontainerapps.io`
- Payments: `https://wallet-payments.kindhill-eee6017a.eastus.azurecontainerapps.io`

**📘 Full Documentation:** See `AZURE_API_ACCESS.md`

---

### 🐳 Local Development (Docker Compose - Optional)

For local development, you can optionally run services via Docker:

| Service | External Port | Internal Port | Container Name | Image |
|---------|---------------|---------------|----------------|-------|
| **Wallet API** | 7015 | 8080 | mciroservices-wallet-service-1 | mciroservices-wallet-service |
| **Identity API** | 7001 | 8080 | mciroservices-identity-service-1 | mciroservices-identity-service |
| **Consent API** | 7002 | 8080 | mciroservices-consent-service-1 | mciroservices-consent-service |
| **Payments API** | 7004 | 8080 | mciroservices-payments-service-1 | mciroservices-payments-service |
| **PostgreSQL** | 5432 | 5432 | mciroservices-postgres-1 | postgres:15 |
| **SQL Server** | 1433 | 1433 | mciroservices-sqlserver-1 | mssql/server:2022-latest |
| **Redis** | 6379 | 6379 | mciroservices-redis-1 | redis:7-alpine |

**Total Containers**: 7

**API Base URLs:**
- Wallet: `http://localhost:7015/api/v1`
- Identity: `http://localhost:7001`
- Consent: `http://localhost:7002`
- Payments: `http://localhost:7004`

**Database Connections:**
- PostgreSQL: `Server=localhost;Port=5432;Database=WalletDB_Dev;`
- SQL Server: `Server=localhost,1433;Database=IdentityDB;`
- Redis: `localhost:6379`

**To switch to local development:**
In `ApiConfiguration.cs`, set `UseAzure = false`

---

## Docker Management Commands

```powershell
# View all running services
docker ps

# View with formatting
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

# View logs for specific service
docker logs mciroservices-wallet-service-1 --tail 50
docker logs mciroservices-identity-service-1 --tail 50
docker logs mciroservices-consent-service-1 --tail 50
docker logs mciroservices-payments-service-1 --tail 50
docker logs mciroservices-postgres-1 --tail 50

# Follow logs in real-time
docker logs -f mciroservices-wallet-service-1

# Restart a service
docker restart mciroservices-wallet-service-1

# Stop all services
docker-compose down

# Start all services
docker-compose up -d

# Check service health
docker inspect mciroservices-wallet-service-1 | Select-String -Pattern "Health"

# Access Swagger UIs
Start-Process "http://localhost:7015/api/v1/wallet/swagger/index.html"
Start-Process "http://localhost:7001/swagger"  # If available
```

---

## Next Steps

1. ✅ Confirm Consent API requirements (if separate service)
2. ✅ Get Identity API OpenAPI/Swagger spec
3. ✅ Verify network accessibility from mobile devices
4. ✅ Set up development PostgreSQL instance
5. ✅ Begin Task 1: Mobile App Project Setup

---

**Document Owner**: Development Team  
**Review Cycle**: Weekly during development
