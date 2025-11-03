# 📋 CredVault Project - Quick Start Guide

**Last Updated**: October 31, 2025

---

## 🎯 What We're Building

A **Digital Wallet Mobile Application** (.NET MAUI 8) that consumes existing microservices:
- **Wallet API** - Credential management, issuance, verification
- **Identity API** - Authentication and user management
- Mobile app provides UI/UX for iOS and Android

---

## 📚 Key Documents

| Document | Purpose | When to Use |
|----------|---------|-------------|
| **DEVELOPMENT_PLAN.md** | Master development plan with 20 tasks | Daily task tracking and implementation reference |
| **ARCHITECTURE.md** | System architecture and data flows | Understanding how systems interact |
| **API_MAPPING.md** | UI screens mapped to API endpoints | Implementing specific features |
| **instructions.md** | Original requirements and API spec | Reference for business requirements |
| **api.md** | Wallet API endpoint list | Quick API endpoint reference |

---

## 🏁 Quick Start Steps

### 1. Verify Prerequisites
```bash
# Check .NET installation
dotnet --version  # Should be 8.0 or higher

# Check MAUI workload
dotnet workload list  # Should show maui

# Install if missing
dotnet workload install maui
```

### 2. Verify Backend Services (Docker)
```powershell
# Check all Docker services are running
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

# Expected containers (7 total):
# - mciroservices-wallet-service-1 (port 7015)
# - mciroservices-identity-service-1 (port 7001)
# - mciroservices-consent-service-1 (port 7002)
# - mciroservices-payments-service-1 (port 7004)
# - mciroservices-postgres-1 (port 5432)
# - mciroservices-sqlserver-1 (port 1433)
# - mciroservices-redis-1 (port 6379)

# Test Wallet API
curl http://localhost:7015/api/v1/wallet/swagger/index.html
# Or open in browser:
Start-Process "http://localhost:7015/api/v1/wallet/swagger/index.html"

# Test Identity API
curl http://localhost:7001

# Test Consent API
curl http://localhost:7002

# Check service health
docker ps | Select-String "healthy"
```

### 3. Start Development
```bash
# Begin with Task 1: Mobile App Project Setup
# See DEVELOPMENT_PLAN.md for detailed steps
```

---

## 🔄 Development Workflow

### Daily Checklist
1. ✅ Review current task in DEVELOPMENT_PLAN.md
2. ✅ Check API_MAPPING.md for endpoint details
3. ✅ Implement feature following acceptance criteria
4. ✅ Test on Android/iOS emulator
5. ✅ Mark task as complete
6. ✅ Commit changes with descriptive message

### Before Each Task
- [ ] Read task deliverables carefully
- [ ] Review dependent tasks
- [ ] Check API endpoints in api.md
- [ ] Review UI designs in stitch_dashboard_home/
- [ ] Set up test data if needed

### After Each Task
- [ ] All acceptance criteria met
- [ ] Code builds without errors
- [ ] Feature tested manually
- [ ] No regressions in existing features
- [ ] Documentation updated if needed

---

## 📊 Current Status

### ✅ Completed
- [x] Requirements analysis
- [x] Architecture planning
- [x] API endpoint documentation
- [x] UI design review
- [x] Development plan creation

### 🔴 Next Up (Task 1)
**Mobile App Project Setup**
- Create .NET MAUI 8 solution
- Configure dependency injection
- Set up MVVM architecture
- Install required NuGet packages

---

## 🛠️ Technology Stack

### Mobile App
- **Framework**: .NET MAUI 8
- **Pattern**: MVVM (Model-View-ViewModel)
- **Navigation**: Shell Navigation
- **DI**: Microsoft.Extensions.DependencyInjection
- **HTTP**: RestSharp or Refit
- **QR Codes**: ZXing.Net.Maui
- **Biometrics**: MAUI Essentials
- **Storage**: SecureStorage (Keychain/KeyStore)

### Backend (Existing - Docker Compose)
- **Wallet Service**: .NET (Port 7015)
- **Identity Service**: GovStack Identity (Port 7001)
- **Consent Service**: .NET (Port 7002)
- **Payments Service**: .NET (Port 7004)
- **PostgreSQL**: Database (Port 5432)
- **SQL Server**: Database (Port 1433)
- **Redis**: Cache (Port 6379)
- **Protocols**: OpenID4VCI, OAuth2, OpenID Connect

---

## 🗂️ Project Structure

```
CredVault/
├── docs/                           # Documentation
│   ├── DEVELOPMENT_PLAN.md         # 20-task development plan
│   ├── ARCHITECTURE.md             # System architecture
│   ├── API_MAPPING.md              # UI-to-API mapping
│   ├── instructions.md             # Original requirements
│   └── api.md                      # API endpoint list
├── stitch_dashboard_home/          # UI designs (HTML prototypes)
│   ├── dashboard_/_home/
│   ├── add_credential_-_select_type_1/
│   ├── credential_details_1/
│   ├── selective_disclosure_-_request_1/
│   ├── verifier_mode_-_scan_home/
│   ├── settings_-_security_1/
│   └── ... (30+ screen designs)
└── src/                            # (To be created in Task 1)
    └── CredVault.Mobile/
        ├── Views/
        ├── ViewModels/
        ├── Services/
        ├── Models/
        └── Resources/
```

---

## 🎓 Key Concepts

### OpenID4VCI Flow
1. **Credential Offer**: Issuer presents credential types available
2. **Authorization**: User authorizes credential issuance
3. **Token Exchange**: Exchange auth code for access token
4. **Credential Issuance**: Receive and store credential

### Verifiable Presentation
1. **Presentation Request**: Verifier requests specific claims
2. **Selective Disclosure**: Holder chooses which claims to share
3. **Presentation Submission**: Submit signed presentation
4. **Verification**: Verifier validates signatures and claims

### Wallet Modes
- **Holder**: Store and present credentials (default)
- **Verifier**: Scan and verify credentials from others
- **Issuer**: Issue credentials to holders

---

## 🔐 Security Checklist

- [ ] All API calls use HTTPS in production
- [ ] JWT tokens stored in SecureStorage (not SharedPreferences)
- [ ] Biometric authentication for sensitive operations
- [ ] Token auto-refresh before expiration
- [ ] Input validation on all user inputs
- [ ] Certificate pinning (production)
- [ ] Proper error handling (no sensitive data in logs)
- [ ] Credential data encrypted at rest

---

## 🧪 Testing Strategy

### Unit Tests
- ViewModels business logic
- API client error handling
- Data model serialization
- Utility functions

### Integration Tests
- API client ↔ Backend communication
- Token refresh flows
- Credential issuance flow end-to-end

### UI Tests
- Navigation flows
- Form validation
- QR code scanning
- Credential display

### Manual Testing
- Test on real iOS device
- Test on real Android device
- Test offline scenarios
- Test biometric authentication

---

## 🚨 Common Issues & Solutions

### Issue: Can't connect to localhost from emulator
**Solution**: 
- Android: Use `10.0.2.2` instead of `localhost`
- iOS: Use actual IP address of dev machine

### Issue: Biometric authentication not working
**Solution**: 
- Ensure device has biometrics enrolled
- Check platform-specific permissions
- Test on real device (not emulator)

### Issue: API returns 401 Unauthorized
**Solution**:
- Check token in SecureStorage
- Verify token not expired
- Ensure Bearer token in Authorization header

### Issue: QR scanning not working
**Solution**:
- Grant camera permissions
- Check ZXing.Net.Maui initialization
- Test with known QR codes first

---

## 📞 Resources

### Documentation
- [.NET MAUI Docs](https://learn.microsoft.com/en-us/dotnet/maui/)
- [OpenID4VCI Spec](https://openid.net/specs/openid-4-verifiable-credential-issuance-1_0.html)
- [W3C Verifiable Credentials](https://www.w3.org/TR/vc-data-model/)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)

### API Endpoints (Docker Services)
- **Wallet API**: http://localhost:7015/api/v1
  - Swagger: http://localhost:7015/api/v1/wallet/swagger/index.html
- **Identity API**: http://localhost:7001
- **Consent API**: http://localhost:7002
- **Payments API**: http://localhost:7004

### Databases
- **PostgreSQL**: localhost:5432 (postgres:15)
- **SQL Server**: localhost:1433 (mssql/server:2022)
- **Redis Cache**: localhost:6379 (redis:7-alpine)

**All Services**: Running in Docker Compose (7 containers total - check `docker ps`)

### Design System
- **Font**: Public Sans
- **Primary Color**: #004aad (Holder), #0b8457 (Verifier), #e88b00 (Issuer)
- **Dark Mode**: Supported
- **Icons**: Material Symbols Outlined

---

## ✅ Ready to Start?

1. Open **DEVELOPMENT_PLAN.md**
2. Find **Task 1: Mobile App Project Setup**
3. Follow deliverables step-by-step
4. Mark checkboxes as you complete them
5. Move to Task 2 when acceptance criteria met

---

**Questions?** Review:
- ARCHITECTURE.md for system design
- API_MAPPING.md for endpoint details
- instructions.md for business requirements

**Ready to code!** 🚀
