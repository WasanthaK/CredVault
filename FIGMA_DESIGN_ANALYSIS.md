# CredVault Figma Design - Complete Analysis

## Overview
After reviewing all Figma design screens in `stitch_dashboard_home/`, this document provides a comprehensive analysis of the intended app design vs. current implementation.

---

## 🎨 Design System

### Color Schemes by Mode

#### **Holder Mode** (Primary - Blue)
- Primary: `#004bad` (Blue)
- Background Light: `#f5f7f8`
- Background Dark: `#0f1823`
- Surface Light: `#ffffff`
- Surface Dark: `#162231`

#### **Verifier Mode** (Green)
- Primary: `#0b8457` / `#20c997` (Green)
- Background Light: `#f6f8f7`
- Background Dark: `#10221b` / `#0a0f1a`
- Card Dark: `#1c2230`

#### **Issuer Assist Mode** (Orange/Amber)
- Primary: `#e88b00` / `#6f42c1` (Orange-Purple)
- Background Light: `#f8f7f5`
- Background Dark: `#231b0f` / `#362917`

### Typography
- Font: **Public Sans** (400, 500, 700, 900 weights)
- Headlines: 32px, bold
- Section Headers: 22px, bold
- Body: 16px, normal
- Caption: 12-14px

### Status Colors
- Success/Active: `#28A745` / `#4CAF50` / `#10B981`
- Warning/Expiring: `#FFC107`
- Error/Revoked: `#DC3545` / `#EF4444` / `#D32F2F`

---

## 📱 App Structure

### **1. Holder Mode** (Default/Primary Mode)

#### **Dashboard Home** (`home_/_dashboard_(holder_mode)__1-5/`)
**Design Elements:**
- Top app bar:
  - Left: Wallet icon + "GovStack Wallet" title
  - Right: "Verified" badge (green checkmark)
- Greeting: "Hi John, you have 4 active credentials."
- Action buttons grid (2 columns):
  - "Add Credential" (primary blue)
  - "Scan QR" (outlined blue)
- Section: "My Credentials"
  - Horizontal scrolling carousel
  - Credential cards (256px width):
    - Issuer logo (circular, 40x40)
    - Credential name (bold)
    - Issuer name (small text)
    - Expiry date
    - Status indicator (dot + text: Active/Expiring Soon/Revoked)
- Section: "Recent Activity"
  - List of recent presentations:
    - Icon (success/error, 40x40 circle)
    - Action description
    - Verifier name + timestamp
- Bottom navigation bar:
  - Home (active)
  - Scan
  - Activity
  - Settings

**Status:** ✅ **Implemented** (DashboardPage.xaml matches design closely)

---

#### **Credential Details** (`credential_details_1-2/`)
**Design Elements:**
- Top bar: Back button, "Credential Details" title
- Credential card:
  - Issuer logo
  - Credential type (bold, large)
  - Divider line
  - Key-value pairs:
    - Full Name
    - Date of Birth
    - ID Number
    - Valid Until
    - Status (colored: Active/Expiring)
- Action buttons (full width):
  - "Verify Status" (primary filled)
  - "Show QR for Verifier" (primary light)
  - "Remove" (text danger)
- QR Modal:
  - Title: "Scan to Verify"
  - Large QR code image
  - Description text
  - Close button
- Remove Confirmation Dialog:
  - "Delete Credential?"
  - Warning text
  - "Cancel" / "Delete" buttons

**Status:** ✅ **Implemented** (CredentialDetailsPage.xaml)

---

#### **Add Credential Flow**

**Step 1: Select Type** (`add_credential_-_select_type_1-2/`)
- Grid of credential type cards:
  - Icon (large, centered)
  - Credential name
  - Issuer name
  - "Popular" badge (optional)
- Grouped by category

**Step 2: Authenticate** (`add_credential_-_authenticate_1-2/`)
- "Redirecting to Gov Portal..." message
- Loading spinner
- OAuth browser redirect

**Step 3: Consent & Review** (`add_credential_-_consent_&_review/`)
- Issuer info card
- List of claims to be issued:
  - Icon (24x24)
  - Claim name
  - Claim value (may be hidden/revealed)
  - Lock icon for sensitive data
- Consent checkbox: "I consent to..."
- "Confirm Issuance" button (disabled until checked)

**Step 4: Confirmation** (`add_credential_-_confirmation_1-2/`)
- Success icon (large checkmark circle)
- "Credential Added!" headline
- Credential name
- "View Credential" / "Back to Dashboard" buttons

**Status:** ✅ **OAuth flow implemented**, UI pages created

---

#### **Selective Disclosure / Presentation** (`selective_disclosure_-_request_1-2/`, `presentation_-_same_device/`, `presentation_-_cross_device/`)

**Sharing Request Screen:**
- Top: "Sharing Request" title
- Requestor info card:
  - Icon (storefront/business)
  - Requestor name (bold)
  - Request description
- "Data from your credential" section:
  - List of claims:
    - Hidden claims: Lock icon + "Hidden" label
    - Requested claims: Checkmark icon + "Requested" label (green)
- Consent checkbox: "I allow only requested data to be shared"
- Action buttons:
  - "Decline" (outlined)
  - "Share" (filled, disabled until checked)

**Preview Screen:**
- Shows exactly what will be shared
- QR code or NFC presentation

**Result Screen:** (`selective_disclosure_-_result_1-2/`)
- Success: "Data Shared Successfully"
- Failure: "Sharing Cancelled"
- Verifier name + timestamp
- "Done" button

**Status:** ⚠️ **Partially implemented** - UI exists, logic incomplete

---

### **2. Verifier Mode** (Green Theme)

#### **Verifier Home** (`verifier_mode_-_scan_home/`)
**Design Elements:**
- Top bar:
  - Verified shield icon (green)
  - "GovStack Wallet" title
  - Settings button
- Headline: "Verifier Mode"
- Body text: "Ready to verify a digital credential."
- Large QR scanner icon (140x140, green background)
- Primary button: "Scan Credential QR"
- Footer:
  - Help text: "Position the QR code inside the frame to scan."
  - Link: "How to verify?"
- **NO bottom navigation** (different from Holder mode!)

**Status:** ⚠️ **Page exists** but not integrated with mode system

---

#### **Scan Result** (`verifier_mode_-_scan_result_1-2/`)

**Success State:**
- Top: "Verification Result" title, "Done" button
- Success card:
  - Green shield icon
  - "Credential Valid" headline
  - "Issued by Gov Authority" subtext
- Details table:
  - Holder Name
  - Credential Type
  - Expiry Date
- "Scan Another Credential" button

**Failure State:**
- Error card (red background):
  - Error icon
  - "Revoked or Expired" headline
  - "Please ask the holder..." message
- "Scan Another Credential" button

**Status:** ⚠️ **Page exists** but needs mode integration

---

#### **Manual Request** (`verifier_mode_-_manual_request/`)
- Form to manually request specific credential types
- Input fields for credential details
- "Send Request" button

**Status:** ⚠️ **Page exists** (ManualRequestPage.xaml)

---

### **3. Issuer Assist Mode** (Orange Theme)

#### **Issuer Assist Form** (`issuer_assist_form/`)
**Design Elements:**
- Top bar: Back button, "Issuer Assist" title
- Headline: "Issue New Credential"
- Body: "Fill in the details below to issue a credential to a holder."
- Form fields:
  - "Holder Identifier" (text input with person icon)
    - Placeholder: "Enter DID or Email"
  - "Credential Type" (dropdown select)
    - Options: Proof of Residency, Driver's License, Academic Transcript
  - "Credential Data" (upload button)
    - "Upload Claim JSON"
    - File upload icon
- Footer:
  - "Issue Credential" button (orange, full width)
- Toast notification (success):
  - Green checkmark
  - "Credential issued successfully."

**Status:** ❌ **NOT IMPLEMENTED** - Page doesn't exist in Views/

---

### **4. Settings & Configuration**

#### **App Mode Switch** (`settings_-_app_mode_switch/`)
**Design Elements:**
- Top: "App Mode" title with back button
- Description: "Select your primary role to tailor the app experience."
- Segmented control (3 options):
  - "Holder" (default selected)
  - "Verifier"
  - "Issuer Assist"
- Mode info cards:
  - **Holder**: Wallet icon (blue), "Manage and present your digital credentials"
  - **Verifier**: Shield icon (green), "Request and verify credentials from others"
  - **Issuer Assist**: Document icon (orange), "Support the issuance process of new credentials"
- Selected card has colored ring indicator
- "Save Changes" button (colored based on selection)

**Status:** ❌ **NOT IMPLEMENTED** - No mode switching in app

---

#### **Other Settings Pages** (`settings_-_*`)
- Security Settings (`security_1-2/`):
  - Biometric authentication toggle
  - PIN settings
  - Auto-lock timer
- Profile & Backup (`profile_&_backup/`):
  - User info
  - Backup options
- Language & Theme (`language_&_theme/`):
  - Language picker
  - Dark/Light mode toggle
- Complaints & Support (`complaints_&_support/`, `complaints_&_data/`):
  - Contact forms
  - Data privacy info

**Status:** ✅ **Implemented** (SettingsPage.xaml) but missing mode switcher

---

## 🔍 Gap Analysis: Design vs Implementation

### ✅ **Fully Implemented**
1. ✅ Dashboard (Holder mode) - Credential carousel, greeting, action buttons
2. ✅ Credential Details - Full display with QR modal
3. ✅ Add Credential Flow - OAuth integration complete
4. ✅ Settings Pages - Language, theme, security, profile, backup
5. ✅ Bottom Navigation - Home, Scan, Activity, Settings tabs

### ⚠️ **Partially Implemented**
1. ⚠️ Verifier Mode UI - Pages exist but not mode-aware
2. ⚠️ Selective Disclosure - UI exists, logic incomplete
3. ⚠️ Presentation Flow - Pages created, needs integration
4. ⚠️ QR Scanner - Page exists, needs mode integration

### ❌ **Missing / Not Implemented**
1. ❌ **App Mode System** - Core feature!
   - No `AppMode` enum
   - No mode switching in Settings
   - No mode persistence (Preferences)
   - No mode-aware theming
   - No mode-specific navigation
2. ❌ **Issuer Assist Mode**
   - IssuerAssistFormPage doesn't exist
   - No issuer workflows
3. ❌ **Mode-Aware Dashboard**
   - Dashboard always shows Holder view
   - Should show different content per mode
4. ❌ **Verifier Mode Integration**
   - Verifier pages not accessible from main navigation
   - No green theme switching
5. ❌ **Status Management** (`status_management/`)
   - Credential status checking UI

---

## 🎯 Implementation Priority

### **Priority 1: App Mode System** (Critical)
Without this, the app is incomplete per Figma design.

**Required Changes:**
1. Create `AppMode` enum:
   ```csharp
   public enum AppMode { Holder, Verifier, IssuerAssist }
   ```
2. Create `AppModeService`:
   - Store current mode in `Preferences`
   - Provide mode change events
   - Theme switching logic
3. Add mode switcher to SettingsPage
4. Update AppShell to show mode-specific home pages
5. Update DashboardPage to show mode-appropriate content

**Estimated Effort:** 4-6 hours

---

### **Priority 2: Mode-Specific Dashboards**
Each mode should have distinct home screen:

**Holder Dashboard** (Current):
- Credential carousel ✅
- Add Credential / Scan QR buttons ✅
- Recent Activity ✅

**Verifier Dashboard** (New):
- Large "Scan Credential QR" button
- Recent verifications list
- Manual request option
- Green theme

**Issuer Assist Dashboard** (New):
- "Issue New Credential" button
- Recent issuances
- Pending requests
- Orange theme

**Estimated Effort:** 3-4 hours

---

### **Priority 3: Verifier Mode Complete Implementation**
Make existing verifier pages functional:

1. ✅ VerifierHomePage exists
2. ✅ QrScannerPage exists
3. ✅ VerificationResultPage exists
4. ⚠️ Need: Integration with QR scanning library
5. ⚠️ Need: Credential verification logic
6. ⚠️ Need: Green theme application

**Estimated Effort:** 4-6 hours

---

### **Priority 4: Issuer Assist Mode**
Currently completely missing:

1. ❌ Create IssuerAssistFormPage.xaml
2. ❌ Create IssuerAssistViewModel
3. ❌ Implement form validation
4. ❌ Implement credential issuance API
5. ❌ Add success toast notification

**Estimated Effort:** 6-8 hours

---

### **Priority 5: Selective Disclosure Complete**
UI exists, needs logic:

1. ✅ SelectiveDisclosurePage UI complete
2. ⚠️ Need: ZKP (Zero-Knowledge Proof) claim selection
3. ⚠️ Need: Selective claim presentation logic
4. ⚠️ Need: Consent tracking

**Estimated Effort:** 4-6 hours

---

## 🎨 Theme System Requirements

### Dynamic Theme Switching by Mode

**Current Implementation:**
- Single blue theme (`#004bad`)
- Light/Dark mode toggle ✅

**Required Implementation:**
```csharp
public static class AppTheme
{
    public static Color GetPrimaryColor(AppMode mode) => mode switch
    {
        AppMode.Holder => Color.FromArgb("#004bad"),      // Blue
        AppMode.Verifier => Color.FromArgb("#20c997"),    // Green
        AppMode.IssuerAssist => Color.FromArgb("#e88b00"), // Orange
        _ => Color.FromArgb("#004bad")
    };
    
    public static Color GetBackgroundLight(AppMode mode) => mode switch
    {
        AppMode.Holder => Color.FromArgb("#f5f7f8"),
        AppMode.Verifier => Color.FromArgb("#f6f8f7"),
        AppMode.IssuerAssist => Color.FromArgb("#f8f7f5"),
        _ => Color.FromArgb("#f5f7f8")
    };
    
    // ... similar for all colors
}
```

**Integration:**
- Update `App.xaml` ResourceDictionaries
- Create `DynamicResource` bindings
- Update on mode change

---

## 📊 User Flows by Mode

### **Holder Mode Flows**
1. **View Credentials**: Dashboard → Credential Details → Actions (QR/Verify/Remove)
2. **Add Credential**: Dashboard → Select Type → OAuth Authenticate → Consent → Confirmation
3. **Share Credential**: Dashboard → Scan (from verifier) → Selective Disclosure → Share/Decline
4. **View Activity**: Dashboard → Activity Tab → Activity Details

### **Verifier Mode Flows**
1. **Scan Credential**: Verifier Home → Scan QR → Verification Result → Done
2. **Manual Request**: Verifier Home → Manual Request → Send Request → Wait for Response
3. **Review History**: Verifier Home → Settings → Activity → Past Verifications

### **Issuer Assist Mode Flows**
1. **Issue Credential**: Issuer Dashboard → Issue Form → Fill Details → Upload JSON → Issue → Success
2. **Review Issued**: Issuer Dashboard → History → Issued Credentials List

---

## 🚀 Recommended Implementation Order

### Phase 1: Core Mode System (Week 1)
1. Create AppMode enum and service
2. Add mode switcher to Settings
3. Implement mode persistence
4. Add mode-aware theme switching

### Phase 2: Verifier Mode (Week 2)
1. Complete QR scanner integration
2. Implement verification logic
3. Apply green theme
4. Test verifier workflows

### Phase 3: Issuer Mode (Week 3)
1. Create IssuerAssistFormPage
2. Implement issuance logic
3. Apply orange theme
4. Test issuer workflows

### Phase 4: Polish & Testing (Week 4)
1. Complete selective disclosure logic
2. Add status management
3. Fix navigation bugs
4. E2E testing all modes

---

## 📝 Key Design Insights

### **1. Mode-Specific Navigation**
- **Holder**: Bottom tabs (Home, Scan, Activity, Settings)
- **Verifier**: NO bottom tabs! Single-focus scan interface
- **Issuer**: Form-focused, minimal navigation

### **2. Color as Identity**
Each mode has distinct color personality:
- Blue = Trust, security (Holder)
- Green = Verification, approval (Verifier)
- Orange = Authority, issuance (Issuer)

### **3. Action Hierarchy**
- **Holder**: Passive (view, manage, present)
- **Verifier**: Active (scan, request, verify)
- **Issuer**: Authoritative (issue, approve, sign)

### **4. Information Density**
- **Holder**: High density (multiple credentials, activity)
- **Verifier**: Low density (focused on single scan)
- **Issuer**: Medium density (form-focused)

---

## ✅ Next Steps

1. **Review this document** with stakeholders
2. **Prioritize** which mode to implement first (suggest: Complete Holder → Add Verifier → Add Issuer)
3. **Create tickets** for each implementation phase
4. **Design API contracts** for verifier/issuer modes
5. **Update UI library** with mode-aware components
6. **Test** each mode independently before integration

---

**Document Version:** 1.0  
**Last Updated:** Based on Figma screens analysis  
**Status:** Ready for implementation planning
