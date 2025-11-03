# 🐳 Docker Services Verification Checklist

**Date**: October 31, 2025

Before starting mobile app development, verify all backend services are running correctly.

---

## 📋 Pre-Development Checklist

### 1. Docker Desktop Running
```powershell
# Check Docker status
docker --version
docker ps

# Expected: Docker version info and container list
```

- [ ] Docker Desktop is running
- [ ] Docker version is up to date

---

### 2. Wallet Service (Port 7015)

```powershell
# Check if Wallet service is running
docker ps | Select-String "wallet"

# Check container health
docker inspect mciroservices-wallet-service-1 --format='{{.State.Health.Status}}'

# Test Wallet API
curl http://localhost:7015/api/v1/wallet/swagger/index.html

# Alternative: Open in browser
Start-Process "http://localhost:7015/api/v1/wallet/swagger/index.html"

# Check logs
docker logs mciroservices-wallet-service-1 --tail 20
```

**Expected Response**: Swagger UI page loads with all API endpoints

- [ ] Wallet container `mciroservices-wallet-service-1` is running
- [ ] Port 7015 is accessible
- [ ] Health status: "healthy"
- [ ] Swagger documentation loads
- [ ] Can see all endpoint categories:
  - [ ] Authorization
  - [ ] Credential
  - [ ] CredentialDiscovery
  - [ ] CredentialIssuance
  - [ ] DeviceTransfer
  - [ ] Holder
  - [ ] Issuer
  - [ ] OpenIDCredentialIssuer
  - [ ] OpenID4VCI
  - [ ] Verifier
  - [ ] Wallet
  - [ ] Workflow

---

### 3. Identity Service (Port 7001)

```powershell
# Check if Identity service is running
docker ps | Select-String "identity"

# Check container health
docker inspect mciroservices-identity-service-1 --format='{{.State.Health.Status}}'

# Test Identity API
curl http://localhost:7001

# Check OpenID configuration (if available)
curl http://localhost:7001/.well-known/openid-configuration

# Check logs
docker logs mciroservices-identity-service-1 --tail 20
```

- [ ] Identity container `mciroservices-identity-service-1` is running
- [ ] Port 7001 is accessible (NOT 5001!)
- [ ] Health status: "healthy"
- [ ] Service responds to requests
- [ ] OpenID configuration available (if applicable)

---

### 4. Consent Service (Port 7002)

```powershell
# Check if Consent service is running
docker ps | Select-String "consent"

# Check container health
docker inspect mciroservices-consent-service-1 --format='{{.State.Health.Status}}'

# Test Consent API
curl http://localhost:7002

# Check logs
docker logs mciroservices-consent-service-1 --tail 20
```

- [ ] Consent container `mciroservices-consent-service-1` is running
- [ ] Port 7002 is accessible
- [ ] Health status: "healthy"
- [ ] Service responds to requests

---

### 5. Payments Service (Port 7004)

```powershell
# Check if Payments service is running
docker ps | Select-String "payments"

# Check container health
docker inspect mciroservices-payments-service-1 --format='{{.State.Health.Status}}'

# Test Payments API
curl http://localhost:7004

# Check logs
docker logs mciroservices-payments-service-1 --tail 20
```

- [ ] Payments container `mciroservices-payments-service-1` is running
- [ ] Port 7004 is accessible
- [ ] Health status: "healthy"
- [ ] Service responds to requests

---

### 6. PostgreSQL Database (Port 5432)

```powershell
# Check if PostgreSQL is running
docker ps | Select-String "postgres"

# Check container status
docker ps --filter "name=mciroservices-postgres-1"

# List databases
docker exec -it mciroservices-postgres-1 psql -U postgres -l

# Check specific database
docker exec -it mciroservices-postgres-1 psql -U postgres -d WalletDB_Dev -c "SELECT version();"

# Check logs
docker logs mciroservices-postgres-1 --tail 20
```

- [ ] PostgreSQL container `mciroservices-postgres-1` is running
- [ ] Image: `postgres:15`
- [ ] Port 5432 is accessible
- [ ] Database `WalletDB_Dev` exists
- [ ] Can connect to database

---

### 7. SQL Server Database (Port 1433)

```powershell
# Check if SQL Server is running
docker ps | Select-String "sqlserver"

# Check container status
docker ps --filter "name=mciroservices-sqlserver-1"

# Test connection (requires sqlcmd)
# sqlcmd -S localhost,1433 -U sa -P YourPassword -Q "SELECT @@VERSION"

# Check logs
docker logs mciroservices-sqlserver-1 --tail 20
```

- [ ] SQL Server container `mciroservices-sqlserver-1` is running
- [ ] Image: `mcr.microsoft.com/mssql/server:2022-latest`
- [ ] Port 1433 is accessible
- [ ] Service is responding

---

### 8. Redis Cache (Port 6379)

```powershell
# Check if Redis is running
docker ps | Select-String "redis"

# Check container status
docker ps --filter "name=mciroservices-redis-1"

# Test Redis connection (requires redis-cli or via Docker)
docker exec -it mciroservices-redis-1 redis-cli ping

# Check Redis info
docker exec -it mciroservices-redis-1 redis-cli info server

# Check logs
docker logs mciroservices-redis-1 --tail 20
```

- [ ] Redis container `mciroservices-redis-1` is running
- [ ] Image: `redis:7-alpine`
- [ ] Port 6379 is accessible
- [ ] Redis responds to PING with PONG

---

### 8. All Services Overview

```powershell
# List all running containers with details
docker ps --format "table {{.Names}}\t{{.Image}}\t{{.Ports}}\t{{.Status}}"

# Check all container logs
docker logs mciroservices-wallet-service-1 --tail 20
docker logs mciroservices-identity-service-1 --tail 20
docker logs mciroservices-consent-service-1 --tail 20
docker logs mciroservices-payments-service-1 --tail 20
docker logs mciroservices-postgres-1 --tail 20
docker logs mciroservices-sqlserver-1 --tail 20
docker logs mciroservices-redis-1 --tail 20

# Check Docker Compose status
docker-compose ps

# Check network
docker network ls
docker network inspect mciroservices_default
```

**Expected Containers:**

| Service | Container Name | Port(s) | Status |
|---------|---------------|---------|--------|
| Wallet API | mciroservices-wallet-service-1 | 7015→8080 | Up (healthy) |
| Identity API | mciroservices-identity-service-1 | 7001→8080 | Up (healthy) |
| Consent API | mciroservices-consent-service-1 | 7002→8080 | Up (healthy) |
| Payments API | mciroservices-payments-service-1 | 7004→8080 | Up (healthy) |
| PostgreSQL | mciroservices-postgres-1 | 5432 | Up |
| SQL Server | mciroservices-sqlserver-1 | 1433 | Up |
| Redis | mciroservices-redis-1 | 6379 | Up |

- [ ] All 7 containers are running
- [ ] No error messages in logs
- [ ] All services show "healthy" or "Up" status
- [ ] Containers are on same Docker network

---

## 🧪 API Endpoint Tests

### Test Wallet API Authorization Endpoint

```powershell
# Test authorization endpoint
curl http://localhost:7015/api/v1/Authorization/test
```

**Expected**: Success response (200 OK)

- [ ] Authorization endpoint responds

---

### Test Wallet API Health Check

```powershell
# Test health endpoint
curl http://localhost:7015/api/v1/Credential/health
```

**Expected**: Health status response

- [ ] Health check passes

---

### Test Credential Query

```powershell
# Test credential query endpoint
curl http://localhost:7015/api/v1/Credential/query
```

**Expected**: Response (may require authentication)

- [ ] Credential endpoints are accessible

---

## 🔧 Troubleshooting

### Issue: Containers not running

```powershell
# Start containers
docker-compose up -d

# Or start individual container
docker start <container-name>
```

### Issue: Port conflicts

```powershell
# Check what's using a port
netstat -ano | findstr :7015
netstat -ano | findstr :5001
netstat -ano | findstr :5432

# Stop conflicting process or change Docker port mapping
```

### Issue: Can't access Swagger

```powershell
# Check Wallet container logs
docker logs <wallet-container-name>

# Restart container
docker restart <wallet-container-name>
```

### Issue: Database connection errors

```powershell
# Check PostgreSQL logs
docker logs <postgres-container-name>

# Verify database exists
docker exec -it <postgres-container-name> psql -U postgres -c "\l"

# Check if WalletDB_Dev exists
docker exec -it <postgres-container-name> psql -U postgres -d WalletDB_Dev -c "SELECT version();"
```

---

## 📊 Service Discovery

### Find Container Names

```powershell
# List all containers
docker ps --all

# Find Wallet container
docker ps | Select-String "wallet"

# Find Identity container
docker ps | Select-String "identity"

# Find PostgreSQL container
docker ps | Select-String "postgres"
```

### View Container Details

```powershell
# Inspect specific container
docker inspect <container-name>

# Get container IP address
docker inspect -f '{{range.NetworkSettings.Networks}}{{.IPAddress}}{{end}}' <container-name>
```

---

## 🚀 Ready for Development

Once all checkboxes are complete:

- [x] All Docker services verified (7 containers)
- [x] Wallet API accessible at `http://localhost:7015/api/v1`
- [x] Swagger UI accessible at `http://localhost:7015/api/v1/wallet/swagger/index.html`
- [x] Identity API accessible at `http://localhost:7001`
- [x] Consent API accessible at `http://localhost:7002`
- [x] Payments API accessible at `http://localhost:7004`
- [x] PostgreSQL accessible at `localhost:5432`
- [x] SQL Server accessible at `localhost:1433`
- [x] Redis accessible at `localhost:6379`
- [x] No errors in container logs

**You're ready to start Task 1: Mobile App Project Setup!**

---

## 📝 Environment Configuration for Mobile App

When creating the mobile app, use these base URLs:

```csharp
// appsettings.json or constants
public static class ApiEndpoints
{
    // Development (localhost)
    public const string WalletBaseUrl = "http://localhost:7015/api/v1";
    public const string IdentityBaseUrl = "http://localhost:7001";
    public const string PaymentsBaseUrl = "http://localhost:7004";
    
    // For Android Emulator, use 10.0.2.2 instead of localhost:
    // public const string WalletBaseUrl = "http://10.0.2.2:7015/api/v1";
    // public const string IdentityBaseUrl = "http://10.0.2.2:7001";
    // public const string PaymentsBaseUrl = "http://10.0.2.2:7004";
    
    // For iOS Simulator, use your machine's IP address:
    // public const string WalletBaseUrl = "http://192.168.1.x:7015/api/v1";
    // public const string IdentityBaseUrl = "http://192.168.1.x:7001";
    // public const string PaymentsBaseUrl = "http://192.168.1.x:7004";
}

public static class DatabaseConnections
{
    public const string PostgreSQL = "Server=localhost;Port=5432;Database=WalletDB_Dev;";
    public const string SqlServer = "Server=localhost,1433;Database=IdentityDB;";
    public const string Redis = "localhost:6379";
}
```

---

## 📞 Additional Resources

- **Wallet Swagger**: http://localhost:7015/api/v1/wallet/swagger/index.html
- **Docker Commands**: `docker ps`, `docker logs`, `docker restart`
- **PostgreSQL UI**: Use pgAdmin or DBeaver to connect to `localhost:5432`

---

**Last Updated**: October 31, 2025  
**Status**: ✅ Ready for verification
