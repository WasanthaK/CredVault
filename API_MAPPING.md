# 🔗 API Endpoint to UI Mapping

This document maps each UI screen/action from the Stitch designs to specific backend API endpoints.

---

## 📱 Dashboard / Home Screen

**File**: `stitch_dashboard_home/dashboard_/_home/code.html`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| Credential Carousel | `GET /api/v1/Credential/holder/{holderId}` | GET | Load all user credentials |
| Credential Status Indicator | `GET /api/v1/Credential/{credentialId}/status` | GET | Check if credential is active/suspended/revoked |
| Activity Feed | `GET /api/v1/Wallet/logs` | GET | Display recent credential activity |
| "Add Credential" Button | Navigate to Add Credential Flow | - | - |
| Backup Icon | Navigate to Backup Screen | - | - |
| Settings Icon | Navigate to Settings Screen | - | - |

---

## ➕ Add Credential Flow

### Select Type Screen
**File**: `stitch_dashboard_home/add_credential_-_select_type_1/code.html`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| Credential Type Grid | `GET /api/v1/Issuer` | GET | List available issuers/credential types |
| "Continue" Button | Proceed to Authentication | - | - |

### Authenticate Screen
**Files**: `add_credential_-_authenticate_1/code.html`, `add_credential_-_authenticate_2/code.html`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| "Authenticate with Issuer" | `GET /api/v1/CredentialDiscovery/credential_offer` | GET | Get credential offer |
| OAuth Redirect | `GET /api/v1/Authorization/authorize` | GET | Initiate OAuth flow |
| Code Exchange | `POST /api/v1/Authorization/token` | POST | Exchange auth code for token |

### Consent & Review Screen
**Files**: `add_credential_-_consent_&_review/code.html`, `add_credential_-_review_&_consent/code.html`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| Display Claims | From credential offer | - | Show what data will be stored |
| "Accept & Add" Button | `POST /api/v1/CredentialIssuance/credential` | POST | Issue credential to holder |

### Confirmation Screen
**Files**: `add_credential_-_confirmation_1/code.html`, `add_credential_-_confirmation_2/code.html`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| "View Credential" | `GET /api/v1/Credential/{credentialId}` | GET | Display credential details |
| "Back to Wallet" | Navigate to Dashboard | - | - |

---

## 📄 Credential Details

**Files**: `credential_details_1/code.html`, `credential_details_2/code.html`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| Load Details | `GET /api/v1/Credential/{credentialId}` | GET | Get full credential data |
| "Verify Status" Button | `POST /api/v1/Credential/{credentialId}/verify` | POST | Verify credential validity |
| Status Badge | `GET /api/v1/Credential/{credentialId}/status` | GET | Show current status |
| "Share" Button | Navigate to Selective Disclosure | - | - |
| "Suspend" Button | `POST /api/v1/Credential/{credentialId}/suspend` | POST | Temporarily suspend credential |
| "Reactivate" Button | `POST /api/v1/Credential/{credentialId}/reactivate` | POST | Reactivate suspended credential |
| "Revoke" Button | `POST /api/v1/Credential/{credentialId}/revoke` | POST | Permanently revoke credential |
| "Delete" Button | `DELETE /api/v1/Credential/{credentialId}` | DELETE | Remove from wallet |

---

## 🔍 Selective Disclosure (Presentation)

### Request Screen
**Files**: `selective_disclosure_-_request_1/code.html`, `selective_disclosure_-_request_2/code.html`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| Display Requested Claims | From presentation definition | - | Show what verifier is requesting |
| Claim Selection Checkboxes | Local UI state | - | User chooses which claims to share |

### Preview Screen
**File**: `selective_disclosure_-_preview/code.html`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| Preview Selected Claims | Local state | - | Show final data to be shared |
| "Confirm & Share" Button | `POST /api/v1/Verifier/verify-presentation` | POST | Submit presentation |
| Biometric Confirmation | Platform API | - | Require biometric before sharing |

### Result Screen
**Files**: `selective_disclosure_-_result_1/code.html`, `selective_disclosure_-_result_2/code.html`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| Verification Result | Response from verify-presentation | - | Display success/failure |
| "View Activity Log" | `GET /api/v1/Wallet/logs` | GET | Show this sharing event |

---

## 📊 Presentation Flows

### Same Device Presentation
**File**: `presentation_-_same_device/code.html`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| "Generate QR" Button | `GET /api/v1/Holder/present/{credentialId}` | GET | Generate presentation QR code |
| Display QR Code | Local QR generation | - | Show code for verifier to scan |

### Cross Device Presentation
**File**: `presentation_-_cross_device/code.html`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| "Scan Verifier QR" | Camera API | - | Scan presentation request |
| Parse Request | `POST /api/v1/Verifier/verify/authorize` | POST | Validate request |
| Submit Response | `POST /api/v1/Verifier/verify/vp-token` | POST | Send presentation token |

---

## ✅ Verifier Mode

### Scan Home
**File**: `verifier_mode_-_scan_home/code.html`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| "Scan Credential QR" | Camera API | - | Scan holder's QR code |
| Parse QR Data | Parse credential/presentation | - | Extract credential data |

### Manual Request
**File**: `verifier_mode_-_manual_request/code.html`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| Select Requested Claims | Local UI | - | Build presentation definition |
| "Generate Request QR" | `POST /api/v1/Verifier/verify/authorize` | POST | Create presentation request |

### Scan Result
**Files**: `verifier_mode_-_scan_result_1/code.html`, `verifier_mode_-_scan_result_2/code.html`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| Verify Scanned Data | `POST /api/v1/Verifier/verify` | POST | Verify credential validity |
| Display Verification Status | Response from verify | - | Show valid/invalid/expired |
| Display Verified Claims | Response data | - | Show credential data |

---

## 🔐 Settings

### Security Settings
**Files**: `settings_-_security_1/code.html`, `settings_-_security_2/code.html`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| "Enable Biometrics" Toggle | Local storage | - | Enable/disable biometric unlock |
| "Require PIN" Toggle | Local storage | - | Enable/disable PIN requirement |
| Settings sync | Could add `PATCH /api/v1/Holder/{id}` | PATCH | Sync settings to backend |

### Backup & Restore
**File**: `settings_-_backup_&_restore/code.html`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| "Export Wallet" Button | `POST /api/v1/DeviceTransfer/export` | POST | Generate encrypted backup |
| Display Backup QR | Local QR generation | - | Show QR for scanning on new device |
| "Import from QR" Button | Camera API | - | Scan backup QR |
| Import Data | `POST /api/v1/DeviceTransfer/import` | POST | Restore credentials |
| Validate Backup | `POST /api/v1/DeviceTransfer/validate` | POST | Verify backup integrity |

### Profile & Backup
**File**: `settings_-_profile_&_backup/code.html`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| Display Profile | `GET /api/v1/Holder/{id}` | GET | Show holder info |
| "Edit Profile" | `PUT /api/v1/Holder/{id}` | PUT | Update holder data |

### Complaints & Support / Data
**Files**: `settings_-_complaints_&_support/code.html`, `settings_-_complaints_&_data/code.html`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| "Report Verifier Misuse" | Could add custom endpoint | POST | Submit complaint |
| View Data Usage | `GET /api/v1/Wallet/logs` | GET | Show credential access history |

### Language & Theme
**File**: `settings_-_language_&_theme/code.html`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| Language Selection | Local storage | - | Change app language |
| Theme Toggle (Dark/Light) | Local storage | - | Switch theme |

### App Mode Switch
**File**: `settings_-_app_mode_switch/code.html`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| Mode Selection (Holder/Verifier/Issuer) | Local storage + API | - | Switch wallet mode |
| Update Mode | Could use `PATCH /api/v1/Holder/{id}` | PATCH | Persist mode preference |

---

## 🏢 Issuer Assist Mode

**File**: `issuer_assist_form/code.html`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| Register as Issuer | `POST /api/v1/Issuer/register` | POST | Register issuer account |
| Issuer Profile | `GET /api/v1/Issuer/{id}` | GET | View issuer details |
| "Issue Credential" Form | `POST /api/v1/Issuer/issue` | POST | Issue credential to holder |
| Select Holder | Search holders (may need endpoint) | GET | Find holder to issue to |
| Workflow Issuance | `POST /api/v1/Workflow/issue` | POST | Use workflow for complex issuance |

---

## 📈 Status Management

**File**: `status_management/code.html`

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| Credential Status List | `GET /api/v1/Credential/holder/{holderId}` | GET | List all credentials with status |
| Bulk Status Check | Loop through credentials | GET | Check each credential status |
| Status Filter | Client-side filtering | - | Filter by active/suspended/revoked |

---

## 📜 Activity Logs

**Accessed from**: Multiple screens (Dashboard, Settings, Credential Details)

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| Activity List | `GET /api/v1/Wallet/logs?page={n}&pageSize=20` | GET | Paginated log entries |
| Log Count | `GET /api/v1/Wallet/logs/count` | GET | Total log entries |
| Credential-Specific Logs | `GET /api/v1/Wallet/credentials/{credentialId}/logs` | GET | Logs for specific credential |
| Filter by Type | Query parameter | GET | Filter by event type |

---

## 📋 Workflow Integration

**Used for**: Complex multi-step credential issuance

| UI Element | API Endpoint | Method | Purpose |
|------------|--------------|--------|---------|
| Start Workflow | `POST /api/v1/Workflow/issue` | POST | Initiate workflow-based issuance |
| Check Workflow Status | `GET /api/v1/Workflow/credentials/{credentialId}` | GET | Monitor issuance progress |
| Citizen Credentials | `GET /api/v1/Workflow/citizens/{citizenSub}/credentials` | GET | List all citizen credentials |

---

## 🔑 Authentication Flows

### Initial Login
```
LoginScreen → Identity API (http://localhost:7001)
    ↓
GET /Holder/{userId} (Wallet API)
    ↓
Dashboard
```

### Token Refresh
```
Expired Token Detected
    ↓
POST /token (with refresh_token)
    ↓
Update stored tokens
    ↓
Retry failed request
```

### Logout
```
Clear SecureStorage tokens
    ↓
Clear cached credentials
    ↓
Navigate to LoginScreen
```

---

## 📊 Data Sync Strategy

| Screen | On Load | On Action | Offline Fallback |
|--------|---------|-----------|------------------|
| Dashboard | Sync credentials + logs | Refresh on pull-down | Show cached data |
| Credential Details | Fetch latest status | Update on verify click | Show last cached version |
| Activity Logs | Fetch recent logs | Load more on scroll | Show cached logs |
| Settings | Load from local storage | Sync to backend on change | Always available |
| Verifier Mode | N/A | Always requires network | Show error if offline |
| Add Credential | Always requires network | - | Block offline |

---

## 🎯 Priority Implementation Order

Based on user flows, implement in this order:

1. **Authentication** (Tasks 4-5) - Must login first
2. **Dashboard** (Task 6) - View credentials
3. **Credential Details** (Task 6) - View/verify single credential
4. **Add Credential Flow** (Task 7) - Issue new credentials
5. **Backup/Restore** (Task 10) - Secure device transfer
6. **Selective Disclosure** (Task 7) - Share credentials
7. **Verifier Mode** (Task 9) - Verify others' credentials
8. **Activity Logs** (Task 12) - Audit trail
9. **Settings** (Task 17) - Preferences
10. **Issuer Mode** (Task 11) - Advanced feature

---

**Last Updated**: October 31, 2025  
**Maintained By**: Development Team
