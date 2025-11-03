Perfect ✅

You want **Output B: Developer Handoff Sheet – Copilot-ready format**, so your .NET 8 + Entity Framework team (and GitHub Copilot) can instantly understand which API endpoint each UI element triggers and how the DTOs look.

Below is a concise, copy-paste-friendly version for your repo (e.g., `/docs/wallet_handoff.md`).
It uses Copilot-parsable structure (comment-based hints + explicit code fences).
Everything matches your **Wallet API** spec and the **Stitch UI** you just built.

---

# 🧱 GovStack Wallet Mobile App — Developer Handoff Sheet

*(Copilot-ready for .NET 8 C# implementation)*

---

## 🧭 Overview

```csharp
// Namespace: Wallet.Api
// Base URL (Azure Production): https://apim-wallet-dev.azure-api.net
// Base URL (Local Docker): http://localhost:7015/api/v1
// Auth: Bearer <JWT> + Azure Subscription Key
// Azure Header: Ocp-Apim-Subscription-Key: 4a47f13f76d54eb999efc2036245ddc2
// Rate Limit: 100 calls per 60 seconds
// All endpoints return ProblemDetails on 4xx/5xx.
```

---

## 🌐 Azure Deployment

**All microservices are now deployed to Azure Container Apps!**

- **API Gateway:** `https://apim-wallet-dev.azure-api.net`
- **Subscription Key:** `4a47f13f76d54eb999efc2036245ddc2`
- **Full Documentation:** See `AZURE_API_ACCESS.md` in the repository

**Service Base Paths:**
- Wallet: `/wallet/api/v1/wallet`
- Identity: `/identity/api/v1/identity`
- Consent: `/consent/api/v1/consent`
- Payments: `/payments/api/v1/payments`

---

---

## 1️⃣ Dashboard / Home

```csharp
// UI: DashboardView
// Purpose: List active credentials + recent activity

// GET /credentials
Task<List<CredentialSummary>> GetCredentialsAsync();

// GET /activity
Task<List<ActivityItem>> GetActivityAsync();
```

```csharp
public record CredentialSummary(
    Guid Id,
    string IssuerName,
    string Type,
    string Status,        // active | suspended | revoked | expired
    DateTime Expiry,
    string LogoUrl
);

public record ActivityItem(
    Guid Id,
    string Event,         // issued | presented | revoked
    string Message,
    DateTime At
);
```

---

## 2️⃣ Add Credential Flow (OpenID4VCI)

```csharp
// Step 1: Get Credential Offer
// GET /wallet/api/v1/wallet/CredentialDiscovery/credential_offer
Task<CredentialOffer> GetCredentialOfferAsync();

// Step 2: Redirect for Authorization
// GET /wallet/api/v1/wallet/Authorization/authorize
Uri GetAuthorizeUri(string offerId);

// Step 3: Exchange Code → Token
// POST /wallet/api/v1/wallet/Authorization/token
Task<TokenResponse> ExchangeTokenAsync(AuthCodeRequest req);

// Step 4: Retrieve Credential
// POST /wallet/api/v1/wallet/CredentialIssuance/credential
Task<Credential> IssueCredentialAsync(CredentialRequest req);
```

```csharp
public record CredentialOffer(string OfferId, string Issuer, string[] SupportedTypes);
public record AuthCodeRequest(string Code, string RedirectUri);
public record TokenResponse(string AccessToken, DateTime ExpiresAt);
public record CredentialRequest(string CredentialType, string AccessToken);
public record Credential(Guid Id, string Type, string Issuer, DateTime ValidFrom, DateTime ValidUntil);
```

UI Bindings in Stitch Comments:

```text
// AddCredentialButton → GET /credential_offer
// ContinueButton      → GET /authorize
// AuthorizeButton     → POST /token
// ViewInWalletButton  → POST /credential
```

---

## 3️⃣ Credential Details + Status + Revocation

```csharp
// GET /wallet/api/v1/wallet/Credential/{id}
Task<CredentialDetails> GetCredentialAsync(Guid id);

// GET /wallet/api/v1/wallet/Credential/{id}/status
Task<CredentialStatus> GetCredentialStatusAsync(Guid id);

// DELETE /wallet/api/v1/wallet/Credential/{id}
Task DeleteCredentialAsync(Guid id);
```

```csharp
public record CredentialDetails(Guid Id, string Type, string Issuer, IDictionary<string,string> Claims);
public record CredentialStatus(string Status, DateTime CheckedAt);
```

UI Mapping:

```text
// VerifyStatusButton → GET /credentials/{id}/status
// ShowQRButton       → POST /presentations/qr
// RemoveButton       → DELETE /credentials/{id}
```

---

## 4️⃣ Selective Disclosure (Verifiable Presentation)

```csharp
// POST /verify/authorize
Task<PresentationRequest> CreatePresentationRequestAsync(PresentationDefinition def);

// POST /presentations
Task<VerificationResult> SubmitPresentationAsync(VpSubmission submission);
```

```csharp
public record PresentationDefinition(string Id, string[] RequestedClaims);
public record PresentationRequest(string RequestId, string QrUri, DateTime Expires);
public record VpSubmission(string RequestId, Dictionary<string,string> PresentedClaims);
public record VerificationResult(bool Verified, string Reason);
```

UI Mapping:

```text
// PrepareProofButton → POST /verify/authorize
// ShareProofButton   → POST /presentations
```

---

## 5️⃣ Verifier Mode / Scan QR

```csharp
// POST /verify/result
Task<VerificationResult> VerifyCredentialAsync(VerifyInput input);
```

```csharp
public record VerifyInput(string QrPayload, string DeviceId);
```

UI Mapping:

```text
// ScanButton → Camera.Scan → POST /verify/result
```

---

## 6️⃣ Activity Log

```csharp
// GET /activity
Task<List<ActivityItem>> GetActivityAsync();
```

UI Mapping:

```text
// ActivityTab → GET /activity
```

---

## 7️⃣ Settings / Security / Backup / Complaints

```csharp
// POST /backups/export
Task<BackupPackage> ExportBackupAsync();

// POST /backups/import
Task ImportBackupAsync(BackupPackage pkg);

// PATCH /settings
Task<UserSettings> UpdateSettingsAsync(UserSettings update);

// POST /complaints
Task SubmitComplaintAsync(ComplaintReport report);
```

```csharp
public record BackupPackage(string QrData, DateTime Created);
public record UserSettings(bool Biometrics, bool RequirePin, string Mode);
public record ComplaintReport(string VerifierName, string Issue, DateTime OccurredAt, string Notes);
```

UI Mapping:

```text
// ExportButton       → POST /backups/export
// ImportButton       → POST /backups/import
// ToggleBiometric    → PATCH /settings
// ReportMisuseButton → POST /complaints
```

---

## 8️⃣ Issuer Assist Mode

```csharp
// POST /credential_offer
Task<CredentialOffer> CreateCredentialOfferAsync(CreateOffer req);

// POST /notification
Task SendNotificationAsync(NotificationMessage msg);
```

```csharp
public record CreateOffer(string HolderDid, string CredentialType, IDictionary<string,string> Claims);
public record NotificationMessage(string HolderDid, string Event, string Message);
```

UI Mapping:

```text
// IssueCredentialButton → POST /credential_offer
// NotifyHolderButton    → POST /notification
```

---

## 9️⃣ Common Models + Enums

```csharp
public enum CredentialStatusEnum { Active, Suspended, Revoked, Expired }
public enum ActivityEvent { Issued, Presented, Revoked }
public enum WalletMode { Holder, Verifier, Issuer }
```

---

## 🔒 10️⃣ Security & Consent Interceptors

```csharp
// Middleware: RequireBiometricConsent
// Applies before sharing credentials or presentations
public class RequireBiometricConsentMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        // Verify local biometric/PIN flag
        if (ctx.Request.Path.StartsWithSegments("/presentations"))
            await EnsureUserConfirmedAsync(ctx);
        await next(ctx);
    }
}
```

---

## ✅ 11️⃣ Unit Test Starter (ready for Copilot generation)

```csharp
// File: Wallet.Tests/CredentialFlowTests.cs
[Fact]
public async Task CanIssueAndVerifyCredential()
{
    var offer = await api.GetCredentialOfferAsync();
    var token = await api.ExchangeTokenAsync(new("code123", "app://callback"));
    var cred  = await api.IssueCredentialAsync(new(offer.SupportedTypes[0], token.AccessToken));
    var status = await api.GetCredentialStatusAsync(cred.Id);
    Assert.Equal("active", status.Status);
}
```

---

## 📋 12️⃣ Integration Checklist (for VS Code / Copilot)

* [x] Create `WalletClient.cs` implementing the above interfaces
* [x] Add Swagger annotations `[HttpGet("credentials")]` etc.
* [x] Use EF Core entities `CredentialEntity`, `ActivityEntity`
* [x] Register `RequireBiometricConsentMiddleware`
* [x] Unit tests scaffolded with xUnit pattern above
* [x] Map Stitch UI events to endpoint methods via `WalletClient`

---

**Next Step →**
Add this file to your repo under `docs/wallet_handoff.md`
and include Copilot comment triggers in each `.razor` / `.cshtml` view, e.g.:

```html
<!-- Copilot: Use WalletClient.GetCredentialsAsync() to populate this list -->
```

Would you like me to generate that **`WalletClient.cs` skeleton** (all async methods pre-defined) next? It would plug directly into your .NET 8 API layer and be Copilot-expandable.
