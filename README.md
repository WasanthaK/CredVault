# 📋 CredVault Project - Quick Start Guide

**Last Updated**: November 11, 2025

---

## 🎯 What We're Building

A **Digital Wallet Mobile Application** (.NET MAUI 9) that consumes existing microservices:
- **Wallet API** - Credential management, issuance, verification
- **Identity API** - Authentication and user management (OAuth/OpenID Connect)
- **Consent API** - Consent and delegation management
- **Payments API** - Payment processing
- Mobile app provides UI/UX for iOS and Android with **production-ready OAuth/PKCE security**

## 📊 Project Status (November 11, 2025)

**Overall Progress: ~50%** ⬆️ (+10% today!)

| Component | Status | Completion | Notes |
|-----------|--------|------------|-------|
| Infrastructure & Architecture | ✅ Complete | 100% | Azure deployed |
| User Authentication | ✅ Complete | 100% | Login/Registration |
| **OAuth/PKCE Flow** | ✅ Complete | 100% | **NEW (Nov 11)** ⬆️ |
| UI/UX Implementation | 🟡 Advanced | 80% | 17 ViewModels, 25+ pages |
| Credential Issuance | 🟡 In Progress | 60% | **↑ from 15%** |
| Credential Management | 🔴 Not Started | 0% | |
| Presentation/Verification | 🔴 Not Started | 0% | |
| Testing | 🔴 Not Started | 0% | |

### ✅ What's Working:
- User registration and login (with Azure APIs)
- **OAuth/PKCE authentication flow** ⬆️ **NEW (Nov 11)**
- **Deep links for OAuth callback** ⬆️ **NEW (Nov 11)**
- **Azure Identity API integration** ⬆️ **NEW (Nov 11)**
- Dashboard navigation and user profile display
- Beautiful GovStack-compliant UI
- All 17 ViewModels and 25+ pages implemented
- Azure Container Apps deployment (4 microservices)

### 🟡 In Progress:
- **Credential Issuance Flow** (60% complete) **↑ from 15%**
  - UI pages designed ✅
  - ViewModels scaffolded ✅
  - **OAuth/PKCE implementation** ✅ **NEW (Nov 11)**
  - **Deep links configured** ✅ **NEW (Nov 11)**
  - **Azure Identity API integrated** ✅ **NEW (Nov 11)**
  - ViewModel integration pending ⏳ **← NEXT STEP**
  - Credential offer retrieval from Wallet API ⏳
  - Secure credential storage ⏳

### 🔴 Not Started:
- Credential revocation/suspension
- QR code presentation
- Selective disclosure
- Backup/restore
- Comprehensive testing

**For detailed status**: See `CURRENT_STATUS.md` and `PROGRESS_REPORT_2025_11_11.md` ⭐

## 🌐 Deployment Status

**✅ All microservices deployed to Azure Container Apps**

- **Environment:** Production (Azure)
- **API Gateway:** Azure API Management (APIM)
- **Base URL:** `https://apim-wallet-dev.azure-api.net`
- **Documentation:** See `AZURE_API_ACCESS.md` for subscription keys and endpoints
- **Test Account:** wasanthak@enadoc.com / Passw0rd!

---

## 📚 Documentation Guide

**📉 Simplified from 16+ documents to 6 core documents! (Nov 11, 2025)**  
See `DOCUMENTATION_CONSOLIDATION_SUMMARY.md` for details.

### Core Documents (Start Here) ⭐
1. **README.md** (this file) - Project overview and quick start
2. **CURRENT_STATUS.md** - Live dashboard (50% progress, updated Nov 11)
3. **IMPLEMENTATION_GUIDE.md** - Step-by-step credential issuance flow ⭐ **NEW**
4. **AZURE_CONFIGURATION.md** - Complete Azure service configuration ⭐ **NEW**
5. **API_REFERENCE.md** - UI to endpoint mapping guide ⭐ **NEW**
6. **ARCHITECTURE.md** - System design and architecture

### Reference Documents
- **TESTING_GUIDE.md** - Manual testing procedures
- **FIGMA_DESIGN_ANALYSIS.md** - UI/UX specifications
- **instructions.md** - Original requirements

### Historical Documents
- **docs/archive/** - Completed reports and superseded documentation

---

## 🚀 Next Steps (Immediate)

**Current Focus: Complete Credential Issuance Flow (60% done!)**

See `IMPLEMENTATION_GUIDE.md` for detailed step-by-step plans.

### This Week (Nov 11-15):
1. ✅ ~~Test Azure Wallet API endpoints~~ **COMPLETE (Nov 11)**
2. ✅ ~~Implement OAuth browser authentication~~ **COMPLETE (Nov 11)**
3. ✅ ~~Configure deep links~~ **COMPLETE (Nov 11)**
4. ⏭️ **Wire up ViewModel integration** ← **NEXT (1-2 hours)**
5. ⏭️ Test OAuth flow on Android emulator
6. ⏭️ Integrate credential offer retrieval from Wallet API
7. ⏭️ Implement secure credential storage
8. ⏭️ Update dashboard to display credentials
9. ⏭️ Test end-to-end flow

### Implementation Phases:
- **Phase 1**: Foundation & platform configuration ✅ **COMPLETE (Nov 11)**
- **Phase 2**: OAuth implementation ✅ **COMPLETE (Nov 11)**
- **Phase 3**: Credential issuance (2-3 days) ← **IN PROGRESS**
- **Phase 4**: Verification & presentation (2 days)
- **Phase 5**: Testing & polish (2 days)
- **Phase 2**: OAuth implementation (1 day)
- **Phase 3**: Credential issuance (1 day)
- **Phase 4**: ViewModel integration (1 day)
- **Phase 5**: Error handling & polish (1 day)
- **Phase 6**: Testing & documentation (1 day)

**Estimated Time**: 3-5 days for complete credential issuance

**For quick status check**: See `CURRENT_STATUS.md`

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

### 2. Verify Backend Services (Azure)

**All services are deployed to Azure! 🚀**

```powershell
# Set your subscription key
$key = "4a47f13f76d54eb999efc2036245ddc2"
$headers = @{ "Ocp-Apim-Subscription-Key" = $key }

# Test Wallet API
Invoke-RestMethod -Uri "https://apim-wallet-dev.azure-api.net/wallet/health" -Headers $headers

# Test Identity API
Invoke-RestMethod -Uri "https://apim-wallet-dev.azure-api.net/identity/health" -Headers $headers

# Test Consent API
Invoke-RestMethod -Uri "https://apim-wallet-dev.azure-api.net/consent/health" -Headers $headers
```

**See `AZURE_API_ACCESS.md` for complete API documentation.**

<details>
<summary>💡 Optional: Run Local Docker Services</summary>

If you need to run services locally for development:

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
Start-Process "http://localhost:7015/api/v1/wallet/swagger/index.html"
```

</details>

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
