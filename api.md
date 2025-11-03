# Wallet API Endpoints

**Base URL:** `http://localhost:7015/api/v1`

**Swagger UI:** `http://localhost:7015/api/v1/wallet/swagger/index.html`

---

## 🐳 Docker Services Overview

All services run in Docker Compose. Container details:

| Service | External Port | Container Name | Status |
|---------|---------------|----------------|--------|
| **Wallet API** | 7015 | mciroservices-wallet-service-1 | ✅ Healthy |
| **Identity API** | 7001 | mciroservices-identity-service-1 | ✅ Healthy |
| **Consent API** | 7002 | mciroservices-consent-service-1 | ✅ Healthy |
| **Payments API** | 7004 | mciroservices-payments-service-1 | ✅ Healthy |
| **PostgreSQL** | 5432 | mciroservices-postgres-1 | ✅ Running |
| **SQL Server** | 1433 | mciroservices-sqlserver-1 | ✅ Running |
| **Redis** | 6379 | mciroservices-redis-1 | ✅ Running |

**Total Containers**: 7

**Verify services:**
```powershell
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
```

## Authorization (`/api/v1/Authorization`)
- `GET /api/v1/Authorization/authorize`
- `POST /api/v1/Authorization/token`
- `POST /api/v1/Authorization/validate`
- `GET /api/v1/Authorization/test`
- `POST /api/v1/Authorization/authorize`
- `GET /api/v1/Authorization/debug`
- `GET /api/v1/Authorization/authorize-test-new`
- `GET /api/v1/Authorization/debug-token`
- `GET /api/v1/Authorization/diagnostics`
- `GET /api/v1/Authorization/authorize-different`

## Credential (`/api/v1/Credential`)
- `POST /api/v1/Credential/issue`
- `GET /api/v1/Credential/{credentialId}`
- `POST /api/v1/Credential/{credentialId}/verify`
- `POST /api/v1/Credential/{credentialId}/revoke`
- `GET /api/v1/Credential/holder/{holderId}`
- `POST /api/v1/Credential/{credentialId}/suspend`
- `POST /api/v1/Credential/{credentialId}/reactivate`
- `GET /api/v1/Credential/{credentialId}/status`
- `DELETE /api/v1/Credential/{credentialId}`
- `POST /api/v1/Credential/query`
- `GET /api/v1/Credential/query`
- `GET /api/v1/Credential/health`

## Credential Discovery (`/api/v1/CredentialDiscovery`)
- `GET /api/v1/CredentialDiscovery/credential_offer`

## Credential Issuance (`/api/v1/CredentialIssuance`)
- `POST /api/v1/CredentialIssuance/credential`
- `POST /api/v1/CredentialIssuance/batch_credential`

## Device Transfer (`/api/v1/DeviceTransfer`)
- `POST /api/v1/DeviceTransfer/export`
- `POST /api/v1/DeviceTransfer/import`
- `POST /api/v1/DeviceTransfer/validate`

## Holder (`/api/v1/Holder`)
- `GET /api/v1/Holder/{id}`
- `POST /api/v1/Holder`
- `PUT /api/v1/Holder/{id}`
- `DELETE /api/v1/Holder/{id}`
- `GET /api/v1/Holder/credential/{credentialId}`
- `GET /api/v1/Holder/present/{credentialId}`

## Issuer (`/api/v1/Issuer`)
- `GET /api/v1/Issuer`
- `GET /api/v1/Issuer/{id}`
- `POST /api/v1/Issuer/register`
- `POST /api/v1/Issuer`
- `PUT /api/v1/Issuer/{id}`
- `DELETE /api/v1/Issuer/{id}`
- `POST /api/v1/Issuer/issue`

## OpenID Credential Issuer (`/api/v1/OpenIDCredentialIssuer`)
- `GET /api/v1/OpenIDCredentialIssuer/openid-credential-issuer`

## OpenID4VCI (`/api/v1/OpenID4VCI`)
- `POST /api/v1/OpenID4VCI/credential`
- `POST /api/v1/OpenID4VCI/batch_credential`
- `GET /api/v1/OpenID4VCI/credential`
- `PUT /api/v1/OpenID4VCI/credential`
- `DELETE /api/v1/OpenID4VCI/credential`
- `PATCH /api/v1/OpenID4VCI/credential`
- `GET /api/v1/OpenID4VCI/batch_credential`
- `PUT /api/v1/OpenID4VCI/batch_credential`
- `DELETE /api/v1/OpenID4VCI/batch_credential`
- `PATCH /api/v1/OpenID4VCI/batch_credential`

## Verifier (`/api/v1/Verifier`)
- `POST /api/v1/Verifier/verify`
- `POST /api/v1/Verifier/verify-presentation`
- `POST /api/v1/Verifier/credential`
- `GET /api/v1/Verifier/verify/{credentialId}`
- `POST /api/v1/Verifier/verify/authorize`
- `POST /api/v1/Verifier/verify/vp-token`

## Wallet (`/api/v1/Wallet`)
- `GET /api/v1/Wallet/logs`
- `GET /api/v1/Wallet/logs/count`
- `GET /api/v1/Wallet/credentials/{credentialId}/logs`

## Workflow (`/api/v1/Workflow`)
- `POST /api/v1/Workflow/issue`
- `GET /api/v1/Workflow/credentials/{credentialId}`
- `GET /api/v1/Workflow/citizens/{citizenSub}/credentials`

---

**Requirements to run:**
- Docker Desktop running
- All 7 containers from Docker Compose:
  - Wallet Service (port 7015)
  - Identity Service (port 7001)
  - Consent Service (port 7002)
  - Payments Service (port 7004)
  - PostgreSQL (port 5432) - `WalletDB_Dev` database
  - SQL Server (port 1433) - Identity/other databases
  - Redis (port 6379) - Caching layer

**Start services:**
```powershell
# Start all services
docker-compose up -d

# Verify all are running (should show 7 containers)
docker ps

# Check health status
docker ps --format "table {{.Names}}\t{{.Status}}"
```

**Stop services:**
```powershell
docker-compose down
```
