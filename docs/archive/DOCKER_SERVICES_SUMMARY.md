# 🎯 Complete Docker Services Configuration

**Last Verified**: October 31, 2025  
**Status**: ✅ All Services Running

---

## 📊 Complete Service Inventory

### Microservices (Docker Compose)

| # | Service | Container Name | Port Mapping | Image | Status |
|---|---------|----------------|--------------|-------|--------|
| 1 | **Wallet API** | mciroservices-wallet-service-1 | 7015→8080 | mciroservices-wallet-service | ✅ Healthy |
| 2 | **Identity API** | mciroservices-identity-service-1 | 7001→8080 | mciroservices-identity-service | ✅ Healthy |
| 3 | **Consent API** | mciroservices-consent-service-1 | 7002→8080 | mciroservices-consent-service | ✅ Healthy |
| 4 | **Payments API** | mciroservices-payments-service-1 | 7004→8080 | mciroservices-payments-service | ✅ Healthy |
| 5 | **PostgreSQL** | mciroservices-postgres-1 | 5432→5432 | postgres:15 | ✅ Running |
| 6 | **SQL Server** | mciroservices-sqlserver-1 | 1433→1433 | mssql/server:2022-latest | ✅ Running |
| 7 | **Redis** | mciroservices-redis-1 | 6379→6379 | redis:7-alpine | ✅ Running |

---

## 🔗 API Endpoints Summary

### Development URLs (Localhost)

```
Wallet Service:
  Base URL: http://localhost:7015/api/v1
  Swagger:  http://localhost:7015/api/v1/wallet/swagger/index.html
  
Identity Service:
  Base URL: http://localhost:7001
  Swagger:  http://localhost:7001/swagger (if available)
  
Consent Service:
  Base URL: http://localhost:7002
  Swagger:  http://localhost:7002/swagger (if available)
  
Payments Service:
  Base URL: http://localhost:7004
  Swagger:  http://localhost:7004/swagger (if available)
```

### Database Connection Strings

```
PostgreSQL:
  Server=localhost;Port=5432;Database=WalletDB_Dev;User Id=postgres;Password=***;
  
SQL Server:
  Server=localhost,1433;Database=IdentityDB;User Id=sa;Password=***;TrustServerCertificate=True;
  
Redis:
  localhost:6379
```

---

## 🏗️ Service Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                     MOBILE APP (.NET MAUI 8)                    │
│  Views (XAML) ◄─► ViewModels (MVVM) ◄─► API Clients + Cache   │
└──────────┬──────────────┬──────────────┬──────────────┬─────────┘
           │              │              │              │
           ▼              ▼              ▼              ▼
    ┌──────────┐   ┌──────────┐   ┌──────────┐   ┌──────────┐
    │  Wallet  │   │ Identity │   │ Consent  │   │ Payments │
    │  :7015   │   │  :7001   │   │  :7002   │   │  :7004   │
    │  🐳      │   │  🐳      │   │  🐳      │   │  🐳      │
    └────┬─────┘   └────┬─────┘   └────┬─────┘   └────┬─────┘
         │              │              │              │
         └──────────────┼──────────────┼──────────────┘
                        │              │
         ┌──────────────┼──────────────┼──────────────┐
         │              │              │              │
         ▼              ▼              ▼              
  ┌──────────┐    ┌──────────┐    ┌──────────┐
  │PostgreSQL│    │SQL Server│    │  Redis   │
  │  :5432   │    │  :1433   │    │  :6379   │
  │  🐳      │    │  🐳      │    │  🐳      │
  └──────────┘    └──────────┘    └──────────┘
```

---

## 🔧 Docker Management

### View All Services
```powershell
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
```

### Check Health Status
```powershell
# Check specific service health
docker inspect mciroservices-wallet-service-1 --format='{{.State.Health.Status}}'
docker inspect mciroservices-identity-service-1 --format='{{.State.Health.Status}}'
docker inspect mciroservices-consent-service-1 --format='{{.State.Health.Status}}'
docker inspect mciroservices-payments-service-1 --format='{{.State.Health.Status}}'

# Should return: "healthy"
```

### View Logs
```powershell
# Real-time logs
docker logs -f mciroservices-wallet-service-1

# Last 50 lines
docker logs mciroservices-wallet-service-1 --tail 50

# All services
docker-compose logs --tail=20
```

### Restart Services
```powershell
# Restart specific service
docker restart mciroservices-wallet-service-1

# Restart all services
docker-compose restart

# Stop and start
docker-compose down
docker-compose up -d
```

---

## 📱 Mobile App Configuration

### For .NET MAUI App

```csharp
// Constants.cs or appsettings.json
public static class ApiConfiguration
{
    // localhost for Windows/Mac development
    public static class Development
    {
        public const string WalletBaseUrl = "http://localhost:7015/api/v1";
        public const string IdentityBaseUrl = "http://localhost:7001";
        public const string ConsentBaseUrl = "http://localhost:7002";
        public const string PaymentsBaseUrl = "http://localhost:7004";
    }
    
    // Android Emulator (10.0.2.2 = host machine)
    public static class AndroidEmulator
    {
        public const string WalletBaseUrl = "http://10.0.2.2:7015/api/v1";
        public const string IdentityBaseUrl = "http://10.0.2.2:7001";
        public const string ConsentBaseUrl = "http://10.0.2.2:7002";
        public const string PaymentsBaseUrl = "http://10.0.2.2:7004";
    }
    
    // iOS Simulator (use actual IP of dev machine)
    public static class IOSSimulator
    {
        public const string WalletBaseUrl = "http://192.168.1.100:7015/api/v1";
        public const string IdentityBaseUrl = "http://192.168.1.100:7001";
        public const string ConsentBaseUrl = "http://192.168.1.100:7002";
        public const string PaymentsBaseUrl = "http://192.168.1.100:7004";
    }
}
```

### Service Registration in MauiProgram.cs

```csharp
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    
    builder.Services.AddMauiBlazorWebView();
    
    // Determine base URLs based on platform
    string walletUrl, identityUrl, consentUrl, paymentsUrl;
    
    #if ANDROID
        walletUrl = ApiConfiguration.AndroidEmulator.WalletBaseUrl;
        identityUrl = ApiConfiguration.AndroidEmulator.IdentityBaseUrl;
        consentUrl = ApiConfiguration.AndroidEmulator.ConsentBaseUrl;
        paymentsUrl = ApiConfiguration.AndroidEmulator.PaymentsBaseUrl;
    #elif IOS
        walletUrl = ApiConfiguration.IOSSimulator.WalletBaseUrl;
        identityUrl = ApiConfiguration.IOSSimulator.IdentityBaseUrl;
        consentUrl = ApiConfiguration.IOSSimulator.ConsentBaseUrl;
        paymentsUrl = ApiConfiguration.IOSSimulator.PaymentsBaseUrl;
    #else
        walletUrl = ApiConfiguration.Development.WalletBaseUrl;
        identityUrl = ApiConfiguration.Development.IdentityBaseUrl;
        consentUrl = ApiConfiguration.Development.ConsentBaseUrl;
        paymentsUrl = ApiConfiguration.Development.PaymentsBaseUrl;
    #endif
    
    // Register HTTP clients
    builder.Services.AddHttpClient<IWalletApiClient, WalletApiClient>(client =>
    {
        client.BaseAddress = new Uri(walletUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    });
    
    builder.Services.AddHttpClient<IIdentityService, IdentityService>(client =>
    {
        client.BaseAddress = new Uri(identityUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    });
    
    builder.Services.AddHttpClient<IConsentService, ConsentService>(client =>
    {
        client.BaseAddress = new Uri(consentUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    });
    
    builder.Services.AddHttpClient<IPaymentsService, PaymentsService>(client =>
    {
        client.BaseAddress = new Uri(paymentsUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    });
    
    return builder.Build();
}
```

---

## ✅ Quick Verification Checklist

Before starting mobile app development:

- [ ] Docker Desktop is running
- [ ] All 7 containers are running: `docker ps | Measure-Object -Line` returns 7+
- [ ] Wallet service is healthy: http://localhost:7015/api/v1/wallet/swagger/index.html loads
- [ ] Identity service is accessible: `curl http://localhost:7001` responds
- [ ] Consent service is accessible: `curl http://localhost:7002` responds
- [ ] Payments service is accessible: `curl http://localhost:7004` responds
- [ ] PostgreSQL is running: `docker ps | Select-String "postgres"`
- [ ] SQL Server is running: `docker ps | Select-String "sqlserver"`
- [ ] Redis is running: `docker ps | Select-String "redis"`
- [ ] No errors in logs: `docker-compose logs --tail=10` shows no exceptions

**Quick Test:**
```powershell
# Run this one-liner to verify all services
docker ps --format "{{.Names}}" | Measure-Object -Line
# Should show: Count = 6
```

---

## 🚨 Troubleshooting

### Services Not Starting

```powershell
# Check Docker daemon
docker info

# View Docker Compose config
docker-compose config

# Start with verbose output
docker-compose up
```

### Port Conflicts

```powershell
# Check what's using ports
netstat -ano | findstr :7015
netstat -ano | findstr :7001
netstat -ano | findstr :7004
netstat -ano | findstr :5432
netstat -ano | findstr :1433
netstat -ano | findstr :6379

# Kill conflicting process (if safe)
Stop-Process -Id <PID>
```

### Container Health Issues

```powershell
# Inspect unhealthy container
docker inspect mciroservices-wallet-service-1

# View detailed logs
docker logs mciroservices-wallet-service-1 --since 10m

# Restart unhealthy container
docker restart mciroservices-wallet-service-1
```

### Database Connection Issues

```powershell
# Test PostgreSQL
docker exec -it mciroservices-postgres-1 psql -U postgres -c "SELECT 1"

# Test SQL Server
docker exec -it mciroservices-sqlserver-1 /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -Q "SELECT @@VERSION"

# Test Redis
docker exec -it mciroservices-redis-1 redis-cli ping
```

---

## 📖 Key Differences from Initial Plan

| Original Assumption | Actual Reality |
|---------------------|----------------|
| Identity on port 5001 | ❌ Actually on port **7001** |
| Only PostgreSQL database | ❌ Have both **PostgreSQL + SQL Server** |
| Only 3 services | ❌ Have **7 containers** (4 APIs + 3 datastores) |
| No payments service | ❌ **Payments service** exists on port 7004 |
| No consent service | ❌ **Consent service** exists on port 7002 |
| No Redis | ❌ **Redis cache** available on port 6379 |

---

## 🎯 Ready for Development

**All systems verified!** ✅

You can now proceed with:
1. **Task 1**: Mobile App Project Setup
2. Use the corrected ports in all API clients
3. Reference DOCKER_VERIFICATION.md for detailed health checks

---

**Document Updated**: October 31, 2025  
**Verified By**: Docker container inspection  
**Next Action**: Begin Task 1 of DEVELOPMENT_PLAN.md
