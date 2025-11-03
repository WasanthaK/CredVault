# Navigation & Mode Audit Against Figma

_Date: 2025-11-01_

This note maps the latest Figma exports under `stitch_dashboard_home/` to the MAUI implementation, highlighting mismatches that explain the "nothing works" navigation experience. Each section records the intended flow, current behaviour, and recommended fixes/tests.

---

## 1. Holder Mode (Blue Theme)

| Design Screen | Expected Route / Behaviour | Current Code Path | Notes |
| --- | --- | --- | --- |
| `home_/_dashboard_(holder_mode)__1` | App launches on holder dashboard with bottom nav. Quick actions: Add, Scan, Activity, Settings. | `AppShell.xaml` only exposes `dashboard` ShellContent, but `App.xaml` still launches `MainPage` (sample button list). Dashboard reachable only via button. | Replace `MainPage` entry point with Shell TabBar, matching design navigation. |
| Quick action "Add Credential" | Go to select type -> OAuth -> Consent -> Confirmation. | `DashboardViewModel.AddCredential` → `NavigateToAsync("addcredential")`. | ✅ Route fixed; flow aligns with design. |
| Quick action "Scan" | Holder scans credential offer (default mode). | `DashboardViewModel.ScanQR` passes `mode="holder"` to `QrScannerViewModel`. `ScanMode` enum lacks `Holder`, so parser fails and defaults; verification commands not triggered. | Add explicit `ScanMode` for holder or omit param; unit-test to catch enum mismatch. |
| Credential tap | Navigate to details page. | `DashboardViewModel.ViewCredential` → `credentialdetails` (lowercase). | ✅ After route casing change. |
| Recent Activity tap | Navigate to detailed log. | `ViewActivity` → `activity`. | ✅ Works. |
| Bottom Nav (Home/Scan/Activity/Settings) | Persistent tabs, colour-coded icons. | Not implemented; MainPage buttons mimic nav instead. | Implement Shell `TabBar` with `ShellContent` per tab; remove MainPage glue. |

### Holder Mode Tests To Add
1. **Dashboard navigation contract**: unit-test `DashboardViewModel` commands with a mock `INavigationService` to assert expected route strings (`addcredential`, `activity`, `settings`, etc.).
2. **Scanner mode parsing**: test `QrScannerViewModel.SetScanMode("Holder")` or adjust command to send `ScanMode.CredentialOffer` object instead of string.

---

## 2. Verifier Mode (Green Theme)

| Design Screen | Expected Route / Behaviour | Current Code Path | Notes |
| --- | --- | --- | --- |
| `verifier_mode_-_scan_home` | Mode switch loads dedicated verifier home (no bottom nav). | `VerifierHomePage` exists, but shell never switches to it; MainPage button calls `Shell.GoToAsync("verifier")`. | Navigation was failing due to DI + bad routes; DI fixed, but shell still keeps holder design layout active. Need mode-aware landing page & theme. |
| "Scan Credential QR" | Opens scanner in verifier mode, auto-wires to result page. | `VerifierViewModel.ScanCredentialQrAsync` → `scanner` route with `mode="VerifierScan"`. Works after route fix. | Ensure `QrScannerViewModel` handles this mode; add test verifying it sets `CurrentScanMode`. |
| "Create Manual Request" | Opens manual request form. | `manualrequest` route now matches Shell registration. | ✅ |
| Result routing | Successful verification should go to `verificationresult`. | `ProcessScannedDataAsync` now points to `verificationresult`. | Functional, but UI uses placeholder data; align with design table layout later. |
| Mode theming | Verifier green palette across page. | `AppModeService` sets CurrentMode but theme resources not switching; background still holder blue palette. | Extend `AppModeService` to push ResourceDictionary changes + write integration test for theme resources if feasible. |

### Verifier Mode Tests To Add
- Mock `INavigationService` with `VerifierViewModel` to assert routes for `ScanCredentialQrCommand`, `CreateManualRequestCommand`, `ScanAnotherCredentialCommand`.
- Unit-test `QrScannerViewModel` verifying `SetScanMode("VerifierScan")` attaches VerifierViewModel and that `ProcessScannedDataAsync` delegates to verifier VM.

---

## 3. Issuer Assist Mode (Orange Theme)

| Design Screen | Expected Route / Behaviour | Current Code Path | Notes |
| --- | --- | --- | --- |
| `issuer_assist_form` | Dedicated Issuer Assist dashboard/form. | No `IssuerAssistPage`; AppMode enum + service exist but UI not implemented. | Build new view + viewmodel; wire to mode switcher. |
| Mode switch (Settings) | Segmented control + cards that persist choice and recolour CTA. | Settings page contains CollectionView of cards (new code), but design expects segmented control & Save button. | Update XAML to match design components; integrate Save action with `AppModeService`. |

### Issuer Mode Tests To Add
- Unit-test `AppModeService` persistence: change mode → store in `Preferences`, raise event.
- UI regression via MVVM tests: `SettingsViewModel.ChangeAppModeCommand` updates `SelectedAppMode` and triggers navigation to new root when implemented.

---

## 4. Cross-Cutting Issues Identified

1. **Entry Point Drift**: `App` still loads `MainPage` (sample). Should instantiate `AppShell` directly with new TabBar structure.
2. **Shell Route Inconsistencies**: Multiple viewmodels previously used route strings that did not exist (`security/pin`, `credentials/add`, etc.). Adjusted to lowercase slugs in this pass; tests will guard against regressions.
3. **Mode Awareness**: `AppModeService` tracks mode but nothing in Shell or ResourceDictionaries responds to it. Need to:
   - Publish events when mode changes.
   - Swap out Shell `FlyoutItem` or TabBar to load mode-specific dashboards.
   - Update `ResourceDictionary` for colours/typography per mode.
4. **Scanner Mode Parameter**: Hard-coded strings (“holder”) do not align with `ScanMode` enum. Replace with enum or constants (e.g., `ScanMode.CredentialOffer`), and unit-test conversions.
5. **Testing Coverage**: No automated tests around navigation or mode transitions. Introducing targeted unit tests will prevent silent regressions.

---

## 5. Recommended Next Steps

1. **Refactor Shell Layout**
   - Remove `MainPage`; configure `AppShell` with TabBar (Holder) and additional `ShellContent` per mode.
   - Hook `AppModeService` to replace the Shell root when mode switches.

2. **Implement Mode-Specific Dashboards**
   - Holder: keep current `DashboardPage` but ensure quick actions match design.
   - Verifier: adjust Shell to navigate to `VerifierHomePage` when mode active.
   - Issuer Assist: create `IssuerAssistPage` aligned with design HTML.

3. **Theme Switching**
   - Extend `App.xaml` resources to include holder/verifier/issuer palettes and update them on mode change.

4. **Testing Suite**
   - Add `NavigationServiceTests` verifying viewmodel route contracts.
   - Add `AppModeServiceTests` covering mode persistence and notifications.
   - (Optional) Use .NET MAUI UITest/integration later for bottom-nav interactions.

5. **Documentation**
   - Update `FIGMA_DESIGN_ANALYSIS.md` once the above is implemented to reflect new status.

---

This document should be revisited after each major navigation or mode change to ensure we stay aligned with the design artefacts.
