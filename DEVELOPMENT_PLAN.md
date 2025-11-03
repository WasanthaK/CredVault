# 🗺️ GovStack Digital Wallet - Development Plan (Single Source of Truth)

**Project**: Digital Wallet Mobile Application (Consumer of Microservices)  
**Status**: Planning Phase  
**Last Updated**: October 31, 2025  
**Tech Stack**: .NET MAUI 8, MVVM, RestSharp/HttpClient, Secure Storage

---

## 🏗️ Microservices Architecture

Our mobile app is a **thin client** consuming existing backend microservices:

| Service | Base URL | Purpose |
|---------|----------|---------|
| **Wallet API** | `http://localhost:7015/api/v1` | Core credential operations, issuance, verification |
| **Identity API** | `http://localhost:7001` | Authentication, user management, tokens |
| **Consent API** | `http://localhost:7002` | User consent management |
| **Payments API** | `http://localhost:7004` | Payment processing |

**Key Point**: We are **NOT building a backend** - we're building a mobile app that integrates with existing APIs.

---

## 📊 Development Progress Dashboard

| Phase | Tasks | Status | Priority |
|-------|-------|--------|----------|
| **Phase 1: Foundation** | Tasks 1-5 | 🔴 Not Started | CRITICAL |
| **Phase 2: Core Features** | Tasks 6-12 | 🔴 Not Started | HIGH |
| **Phase 3: Advanced Features** | Tasks 13-17 | 🔴 Not Started | MEDIUM |
| **Phase 4: UI/UX Implementation** | Tasks 18-19 | 🔴 Not Started | HIGH |
| **Phase 5: Testing & Polish** | Task 20 | 🔴 Not Started | MEDIUM |

**Legend**: 🔴 Not Started | 🟡 In Progress | 🟢 Complete | ⚠️ Blocked

---

## 🎯 Phase 1: Foundation (Week 1)

### ✅ Task 1: Mobile App Project Setup
**Priority**: CRITICAL | **Estimated Time**: 3 hours

**Deliverables**:
- [ ] Verify Docker services are running (7 containers total):
  ```bash
  docker ps
  # Verify Wallet API: http://localhost:7015/api/v1/wallet/swagger/index.html
  # Verify Identity API: http://localhost:7001
  # Verify Consent API: http://localhost:7002
  # Verify Payments API: http://localhost:7004
  # Verify PostgreSQL: localhost:5432
  # Verify SQL Server: localhost:1433
  # Verify Redis: localhost:6379
  ```
- [ ] Create .NET MAUI 8 solution:
  ```
  CredVault.sln
  ├── src/
  │   └── CredVault.Mobile/           # .NET MAUI App
  │       ├── Views/                  # XAML Pages
  │       ├── ViewModels/             # MVVM ViewModels
  │       ├── Services/               # API clients, storage
  │       ├── Models/                 # DTOs, domain models
  │       ├── Converters/             # Value converters
  │       └── Resources/              # Images, styles
  └── tests/
      └── CredVault.Mobile.Tests/     # Unit tests
  ```
- [ ] Add NuGet packages:
  - `CommunityToolkit.Mvvm` (MVVM helpers)
  - `CommunityToolkit.Maui` (UI components)
  - `RestSharp` or `Refit` (HTTP client)
  - `ZXing.Net.Maui` (QR scanning)
  - `Newtonsoft.Json` or `System.Text.Json`
- [ ] Configure dependency injection in `MauiProgram.cs`:
  ```csharp
  builder.Services.AddSingleton<IWalletApiClient>(sp => 
      new WalletApiClient("http://localhost:7015/api/v1"));
  builder.Services.AddSingleton<IIdentityService>(sp =>
      new IdentityService("http://localhost:7001"));
  builder.Services.AddSingleton<IConsentService>(sp =>
      new ConsentService("http://localhost:7002"));
  builder.Services.AddSingleton<IPaymentsService>(sp =>
      new PaymentsService("http://localhost:7004"));
  ```
- [ ] Set up MVVM navigation service
- [ ] Create base ViewModel class with INotifyPropertyChanged

**Acceptance Criteria**:
- Docker services verified and accessible
- App launches on Android/iOS emulator
- DI container configured correctly
- Can navigate between pages
- Base architecture in place

**Dependencies**: None

---

### ✅ Task 2: API Client Models & DTOs
**Priority**: CRITICAL | **Estimated Time**: 4 hours

**Deliverables**:
- [ ] Create C# models in `CredVault.Mobile/Models/` matching Wallet API responses:
  ```csharp
  // Credential.cs
  public class Credential
  {
      public Guid CredentialId { get; set; }
      public string Type { get; set; }
      public string Issuer { get; set; }
      public string Status { get; set; } // active, suspended, revoked
      public DateTime IssuedAt { get; set; }
      public DateTime ExpiresAt { get; set; }
      public Dictionary<string, object> Claims { get; set; }
  }
  
  // Holder.cs
  public class Holder
  {
      public Guid Id { get; set; }
      public string Sub { get; set; } // Citizen identifier
      public string Name { get; set; }
      public DateTime CreatedAt { get; set; }
  }
  
  // CredentialOffer.cs
  public class CredentialOffer
  {
      public string CredentialIssuer { get; set; }
      public string[] CredentialTypes { get; set; }
      public Dictionary<string, object> Grants { get; set; }
  }
  
  // VerificationResult.cs
  public class VerificationResult
  {
      public bool IsValid { get; set; }
      public string Status { get; set; }
      public Dictionary<string, object> VerifiedClaims { get; set; }
      public DateTime VerifiedAt { get; set; }
  }
  
  // DeviceTransferRequest.cs / DeviceTransferResponse.cs
  // AuthorizationRequest.cs / TokenResponse.cs
  // IssuerRegistration.cs
  ```
- [ ] Create request/response wrappers for all API endpoints
- [ ] Add JSON serialization attributes
- [ ] Document each model with XML comments

**Acceptance Criteria**:
- All models match Wallet API schema
- JSON serialization works correctly
- Models are strongly typed
- Clear documentation for each property

**Dependencies**: Task 1

### ✅ Task 3: WalletApiClient Service
**Priority**: CRITICAL | **Estimated Time**: 6 hours

**Deliverables**:
- [ ] Create `IWalletApiClient` interface in `Services/`:
  ```csharp
  public interface IWalletApiClient
  {
      // Authorization
      Task<AuthorizationResponse> AuthorizeAsync(AuthorizationRequest request);
      Task<TokenResponse> GetTokenAsync(TokenRequest request);
      Task<bool> ValidateTokenAsync(string token);
      
      // Credentials
      Task<Credential> IssueCredentialAsync(IssueCredentialRequest request);
      Task<Credential> GetCredentialAsync(Guid credentialId);
      Task<VerificationResult> VerifyCredentialAsync(Guid credentialId);
      Task RevokeCredentialAsync(Guid credentialId);
      Task<List<Credential>> GetHolderCredentialsAsync(Guid holderId);
      Task SuspendCredentialAsync(Guid credentialId);
      Task ReactivateCredentialAsync(Guid credentialId);
      Task<CredentialStatus> GetCredentialStatusAsync(Guid credentialId);
      Task DeleteCredentialAsync(Guid credentialId);
      
      // Credential Discovery & Issuance
      Task<CredentialOffer> GetCredentialOfferAsync();
      Task<Credential> RequestCredentialAsync(CredentialRequest request);
      
      // Holder
      Task<Holder> GetHolderAsync(Guid id);
      Task<Holder> CreateHolderAsync(CreateHolderRequest request);
      Task<Holder> UpdateHolderAsync(Guid id, UpdateHolderRequest request);
      
      // Issuer
      Task<List<Issuer>> GetIssuersAsync();
      Task<Issuer> RegisterIssuerAsync(IssuerRegistration request);
      
      // Verifier
      Task<VerificationResult> VerifyPresentationAsync(VerifyPresentationRequest request);
      Task<AuthorizationResponse> VerifyAuthorizeAsync(PresentationDefinition definition);
      
      // Device Transfer (Backup/Restore)
      Task<DeviceTransferResponse> ExportAsync(DeviceTransferRequest request);
      Task ImportAsync(DeviceTransferRequest request);
      
      // Wallet Logs
      Task<List<WalletLog>> GetLogsAsync(int page = 1, int pageSize = 20);
      Task<int> GetLogsCountAsync();
      
      // Workflow
      Task<WorkflowResult> IssueWorkflowAsync(IssueWorkflowRequest request);
  }
  ```
- [ ] Implement `WalletApiClient` using HttpClient
- [ ] Add base URL configuration: `http://localhost:7015/api/v1`
- [ ] Implement authentication header injection
- [ ] Add retry logic and timeout handling
- [ ] Add request/response logging

**Acceptance Criteria**:
- All Wallet API endpoints wrapped
- Proper error handling for network failures
- Authentication tokens automatically included
- Can successfully call test endpoints

**Dependencies**: Tasks 1, 2

### ✅ Task 4: Identity Service Integration
**Priority**: CRITICAL | **Estimated Time**: 5 hours

**Deliverables**:
- [ ] Create `IIdentityService` interface:
  ```csharp
  public interface IIdentityService
  {
      Task<LoginResult> LoginAsync(string username, string password);
      Task<LoginResult> LoginWithBiometricsAsync();
      Task LogoutAsync();
      Task<TokenResponse> RefreshTokenAsync(string refreshToken);
      Task<bool> ValidateTokenAsync();
      Task<UserProfile> GetCurrentUserAsync();
      string GetAccessToken();
      bool IsAuthenticated { get; }
  }
  ```
- [ ] Implement `IdentityService` calling GovStack Identity API (`http://localhost:7001`)
- [ ] Create token storage service using SecureStorage:
  ```csharp
  public interface ITokenStorageService
  {
      Task SaveTokenAsync(string accessToken, string refreshToken);
      Task<string> GetAccessTokenAsync();
      Task<string> GetRefreshTokenAsync();
      Task ClearTokensAsync();
  }
  ```
- [ ] Implement automatic token refresh when expired
- [ ] Add authentication state management (logged in/out)
- [ ] Create login/logout ViewModels
- [ ] Add HTTP interceptor to inject Bearer token

**Acceptance Criteria**:
- User can log in with Identity service
- Tokens stored securely in platform keychain
- Expired tokens auto-refresh
- All API calls include authentication headers
- Logout clears all stored credentials

**Dependencies**: Tasks 1, 2, 3

### ✅ Task 5: Authorization Flow Implementation  
**Priority**: CRITICAL | **Estimated Time**: 4 hours

**Deliverables**:
- [ ] Implement OAuth2/OpenID4VCI authorization flow:
  ```csharp
  // Step 1: Initiate authorization
  var authRequest = new AuthorizationRequest
  {
      ResponseType = "code",
      ClientId = "credvault-mobile",
      RedirectUri = "credvault://callback",
      Scope = "openid profile credential",
      CodeChallenge = GeneratePKCEChallenge(),
      CodeChallengeMethod = "S256"
  };
  var authResponse = await _walletClient.AuthorizeAsync(authRequest);
  
  // Step 2: Exchange code for token
  var tokenRequest = new TokenRequest
  {
      GrantType = "authorization_code",
      Code = authResponse.Code,
      RedirectUri = "credvault://callback",
      CodeVerifier = pkceVerifier
  };
  var token = await _walletClient.GetTokenAsync(tokenRequest);
  ```
- [ ] Implement PKCE (Proof Key for Code Exchange)
- [ ] Handle OAuth redirect callback
- [ ] Validate tokens with `/api/v1/Authorization/validate`
- [ ] Add authorization error handling

**Acceptance Criteria**:
- Complete OAuth2 flow works end-to-end
- PKCE prevents code interception attacks
- Token validation succeeds
- Errors displayed to user appropriately

**Dependencies**: Tasks 3, 4

---

## 🚀 Phase 2: Core Features (Week 2)

### ✅ Task 5: Credentials API - CRUD Operations
**Priority**: HIGH | **Estimated Time**: 5 hours

**Deliverables**:
- [ ] Create `CredentialsController.cs`:
  ```csharp
  [ApiController]
  [Route("api/v1/credentials")]
  [Authorize]
  public class CredentialsController : ControllerBase
  {
      // GET /api/v1/credentials
      [HttpGet]
      public async Task<ActionResult<List<CredentialSummary>>> GetCredentialsAsync()
      
      // GET /api/v1/credentials/{id}
      [HttpGet("{id}")]
      public async Task<ActionResult<CredentialDetails>> GetCredentialAsync(Guid id)
      
      // GET /api/v1/credentials/{id}/status
      [HttpGet("{id}/status")]
      public async Task<ActionResult<CredentialStatus>> GetCredentialStatusAsync(Guid id)
      
      // DELETE /api/v1/credentials/{id}
      [HttpDelete("{id}")]
      public async Task<IActionResult> DeleteCredentialAsync(Guid id)
  }
  ```
- [ ] Implement `ICredentialService` and `CredentialService`
- [ ] Add status check logic (verify against issuer endpoint)
- [ ] Implement soft delete or permanent delete strategy
- [ ] Add filtering and sorting to GET endpoint

**Acceptance Criteria**:
- All endpoints return proper HTTP status codes
- User can only access their own credentials
- Status check contacts issuer API correctly
- Deletion triggers activity log entry

**Dependencies**: Tasks 2, 3, 4

---

### ✅ Task 6: OpenID4VCI Flow - Credential Issuance
**Priority**: HIGH | **Estimated Time**: 8 hours

**Deliverables**:
- [ ] Create `IssuanceController.cs`:
  ```csharp
  // Step 1: Initiate flow
  [HttpGet("credential_offer")]
  public async Task<ActionResult<CredentialOffer>> GetCredentialOfferAsync()
  
  // Step 2: Authorization redirect
  [HttpGet("authorize")]
  public IActionResult GetAuthorizeUri([FromQuery] string offerId)
  
  // Step 3: Token exchange
  [HttpPost("token")]
  public async Task<ActionResult<TokenResponse>> ExchangeTokenAsync(AuthCodeRequest req)
  
  // Step 4: Issue credential
  [HttpPost("credential")]
  public async Task<ActionResult<Credential>> IssueCredentialAsync(CredentialRequest req)
  ```
- [ ] Implement OAuth2 authorization code flow
- [ ] Create credential offer state management (Redis/in-memory cache)
- [ ] Add issuer discovery and metadata validation
- [ ] Implement credential format transformation (SD-JWT, JWT-VC)
- [ ] Add proof of possession validation

**Acceptance Criteria**:
- Complete flow from offer → auth → token → credential works
- State is properly managed across steps
- Invalid or expired offers are rejected
- Issued credentials are stored in database

**Dependencies**: Tasks 2, 3, 4, 5

---

### ✅ Task 7: Verification & Presentation APIs
**Priority**: HIGH | **Estimated Time**: 6 hours

**Deliverables**:
- [ ] Create `VerificationController.cs`:
  ```csharp
  // Create presentation request
  [HttpPost("verify/authorize")]
  public async Task<ActionResult<PresentationRequest>> CreatePresentationRequestAsync(
      PresentationDefinition def)
  
  // Verify scanned QR
  [HttpPost("verify/result")]
  public async Task<ActionResult<VerificationResult>> VerifyCredentialAsync(
      VerifyInput input)
  ```
- [ ] Create `PresentationsController.cs`:
  ```csharp
  // Submit presentation
  [HttpPost("presentations")]
  public async Task<ActionResult<VerificationResult>> SubmitPresentationAsync(
      VpSubmission submission)
  
  // Generate QR for presentation
  [HttpPost("presentations/qr")]
  public async Task<ActionResult<QrCodeResponse>> GenerateQrAsync(Guid credentialId)
  ```
- [ ] Implement selective disclosure logic
- [ ] Add DIF Presentation Exchange support
- [ ] Verify cryptographic signatures
- [ ] Generate QR codes with presentation data

**Acceptance Criteria**:
- Verifier can request specific claims
- Holder can choose which claims to share
- Presentation signatures are validated
- QR codes contain proper VP format

**Dependencies**: Tasks 5, 6

---

### ✅ Task 8: Activity Log Service
**Priority**: HIGH | **Estimated Time**: 3 hours

**Deliverables**:
- [ ] Create `ActivityController.cs`:
  ```csharp
  [HttpGet("activity")]
  public async Task<ActionResult<List<ActivityItem>>> GetActivityAsync(
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 20,
      [FromQuery] ActivityEvent? eventType = null)
  ```
- [ ] Implement `IActivityService` with logging methods
- [ ] Add automatic activity creation on credential events
- [ ] Implement pagination and filtering
- [ ] Add activity aggregation (daily summaries)

**Acceptance Criteria**:
- All credential operations create activity entries
- Activity list is paginated properly
- User can filter by event type
- Activity includes timestamps and credential references

**Dependencies**: Tasks 5, 6, 7

---

## 🔧 Phase 3: Advanced Features (Week 3)

### ✅ Task 9: Settings & Security Management
**Priority**: HIGH | **Estimated Time**: 4 hours

**Deliverables**:
- [ ] Create `SettingsController.cs`:
  ```csharp
  [HttpGet("settings")]
  public async Task<ActionResult<UserSettings>> GetSettingsAsync()
  
  [HttpPatch("settings")]
  public async Task<ActionResult<UserSettings>> UpdateSettingsAsync(
      UserSettingsUpdate update)
  ```
- [ ] Implement biometric toggle with device registration
- [ ] Add PIN requirement configuration
- [ ] Implement wallet mode switching (Holder/Verifier/Issuer)
- [ ] Add theme and language preferences

**Acceptance Criteria**:
- Settings persist per user
- Mode switching affects available endpoints
- Biometric settings integrate with middleware
- Changes are audited in activity log

**Dependencies**: Tasks 4, 8

---

### ✅ Task 10: Backup & Restore Functionality
**Priority**: HIGH | **Estimated Time**: 5 hours

**Deliverables**:
- [ ] Create `BackupController.cs`:
  ```csharp
  [HttpPost("backups/export")]
  public async Task<ActionResult<BackupPackage>> ExportBackupAsync()
  
  [HttpPost("backups/import")]
  public async Task<IActionResult> ImportBackupAsync(BackupPackage pkg)
  ```
- [ ] Implement encrypted backup serialization
- [ ] Generate QR code containing encrypted data
- [ ] Add backup versioning and compatibility checks
- [ ] Implement restore with conflict resolution
- [ ] Add backup verification and integrity checks

**Acceptance Criteria**:
- Backup includes all credentials and settings
- Data is encrypted before export
- QR code can be scanned to restore
- Import validates backup format and version
- Restore creates activity log entry

**Dependencies**: Tasks 5, 9

---

### ✅ Task 11: Complaints & Reporting System
**Priority**: MEDIUM | **Estimated Time**: 3 hours

**Deliverables**:
- [ ] Create `ComplaintsController.cs`:
  ```csharp
  [HttpPost("complaints")]
  public async Task<IActionResult> SubmitComplaintAsync(ComplaintReport report)
  
  [HttpGet("complaints")]
  public async Task<ActionResult<List<ComplaintSummary>>> GetComplaintsAsync()
  ```
- [ ] Implement complaint validation and storage
- [ ] Add verifier identification and lookup
- [ ] Create complaint status workflow
- [ ] Add admin notification system

**Acceptance Criteria**:
- Users can report verifier misuse
- Complaints include verifier details and incident description
- System tracks complaint status
- Admins receive notifications for new complaints

**Dependencies**: Task 8

---

### ✅ Task 12: Issuer Assist Mode APIs
**Priority**: MEDIUM | **Estimated Time**: 4 hours

**Deliverables**:
- [ ] Create `IssuerController.cs`:
  ```csharp
  [HttpPost("credential_offer")]
  [Authorize(Policy = "IssuerMode")]
  public async Task<ActionResult<CredentialOffer>> CreateCredentialOfferAsync(
      CreateOffer req)
  
  [HttpPost("notification")]
  public async Task<IActionResult> SendNotificationAsync(
      NotificationMessage msg)
  ```
- [ ] Implement holder DID lookup
- [ ] Add credential template system
- [ ] Create notification delivery service
- [ ] Add issuer authorization checks

**Acceptance Criteria**:
- Issuers can create credential offers for holders
- Holders receive push notifications
- Credential templates enforce schema
- Only authorized issuers can use endpoints

**Dependencies**: Tasks 6, 9

---

## 🎨 Phase 4: Client & Quality Assurance (Week 4)

### ✅ Task 13: WalletClient Service Layer
**Priority**: MEDIUM | **Estimated Time**: 4 hours

**Deliverables**:
- [ ] Create `WalletClient.cs` in `CredVault.Application/Services/`:
  ```csharp
  public class WalletClient : IWalletClient
  {
      private readonly HttpClient _httpClient;
      private readonly ITokenProvider _tokenProvider;
      
      public Task<List<CredentialSummary>> GetCredentialsAsync();
      public Task<CredentialDetails> GetCredentialAsync(Guid id);
      public Task<CredentialOffer> GetCredentialOfferAsync();
      public Task<TokenResponse> ExchangeTokenAsync(AuthCodeRequest req);
      public Task<Credential> IssueCredentialAsync(CredentialRequest req);
      public Task<PresentationRequest> CreatePresentationRequestAsync(PresentationDefinition def);
      public Task<VerificationResult> SubmitPresentationAsync(VpSubmission submission);
      public Task<List<ActivityItem>> GetActivityAsync();
      public Task<BackupPackage> ExportBackupAsync();
      public Task ImportBackupAsync(BackupPackage pkg);
      public Task<UserSettings> UpdateSettingsAsync(UserSettings update);
      public Task SubmitComplaintAsync(ComplaintReport report);
      // ... all other methods
  }
  ```
- [ ] Add retry policies with Polly
- [ ] Implement response caching
- [ ] Add request/response logging
- [ ] Create mock client for testing

**Acceptance Criteria**:
- Client wraps all API endpoints
- Proper error handling and retries
- Can be easily consumed by UI layer
- Includes XML documentation

**Dependencies**: All API tasks (5-12)

---

### ✅ Task 14: Error Handling & ProblemDetails
**Priority**: MEDIUM | **Estimated Time**: 3 hours

**Deliverables**:
- [ ] Create `GlobalExceptionHandlerMiddleware.cs`:
  ```csharp
  public async Task InvokeAsync(HttpContext context, RequestDelegate next)
  {
      try
      {
          await next(context);
      }
      catch (Exception ex)
      {
          await HandleExceptionAsync(context, ex);
      }
  }
  
  private async Task HandleExceptionAsync(HttpContext context, Exception ex)
  {
      var problemDetails = ex switch
      {
          ValidationException => new ProblemDetails 
          { 
              Status = 400, 
              Title = "Validation Error" 
          },
          NotFoundException => new ProblemDetails 
          { 
              Status = 404, 
              Title = "Resource Not Found" 
          },
          // ... more mappings
          _ => new ProblemDetails 
          { 
              Status = 500, 
              Title = "Internal Server Error" 
          }
      };
      
      context.Response.StatusCode = problemDetails.Status.Value;
      await context.Response.WriteAsJsonAsync(problemDetails);
  }
  ```
- [ ] Implement custom exception types
- [ ] Add correlation IDs to responses
- [ ] Create development vs production error details
- [ ] Add exception logging

**Acceptance Criteria**:
- All exceptions return RFC 7807 format
- 4xx errors include validation details
- 5xx errors hide sensitive information
- Correlation IDs enable tracing

**Dependencies**: Task 1

---

### ✅ Task 15: Swagger/OpenAPI Documentation
**Priority**: MEDIUM | **Estimated Time**: 3 hours

**Deliverables**:
- [ ] Configure Swashbuckle in `Program.cs`:
  ```csharp
  builder.Services.AddSwaggerGen(options =>
  {
      options.SwaggerDoc("v1", new OpenApiInfo
      {
          Title = "GovStack Wallet API",
          Version = "v1",
          Description = "Digital Wallet API for Verifiable Credentials"
      });
      
      options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
      {
          Type = SecuritySchemeType.Http,
          Scheme = "bearer",
          BearerFormat = "JWT"
      });
      
      var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
      options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
  });
  ```
- [ ] Add XML documentation to all endpoints
- [ ] Create example request/response bodies
- [ ] Add operation descriptions and tags
- [ ] Generate OpenAPI 3.0 specification file

**Acceptance Criteria**:
- Swagger UI shows all endpoints
- JWT authentication works in Swagger
- Examples are helpful and accurate
- OpenAPI spec validates successfully

**Dependencies**: All API tasks (5-12)

---

### ✅ Task 16: Unit Tests - Core Business Logic
**Priority**: MEDIUM | **Estimated Time**: 6 hours

**Deliverables**:
- [ ] Create test project with xUnit, FluentAssertions, Moq
- [ ] Implement `CredentialServiceTests.cs`:
  ```csharp
  public class CredentialServiceTests
  {
      [Fact]
      public async Task GetCredentials_ReturnsOnlyUserCredentials()
      
      [Fact]
      public async Task DeleteCredential_CreatesActivityLogEntry()
      
      [Fact]
      public async Task GetCredentialStatus_ContactsIssuerApi()
  }
  ```
- [ ] Implement `IssuanceFlowTests.cs`:
  ```csharp
  [Fact]
  public async Task CanIssueAndVerifyCredential()
  {
      var offer = await service.GetCredentialOfferAsync();
      var token = await service.ExchangeTokenAsync(new("code123", "app://callback"));
      var cred  = await service.IssueCredentialAsync(new(offer.SupportedTypes[0], token.AccessToken));
      var status = await service.GetCredentialStatusAsync(cred.Id);
      Assert.Equal("active", status.Status);
  }
  ```
- [ ] Test selective disclosure logic
- [ ] Test backup encryption/decryption
- [ ] Test validation rules

**Acceptance Criteria**:
- Code coverage > 80% for business logic
- All critical paths have tests
- Tests use proper mocking
- Tests are fast and isolated

**Dependencies**: Tasks 5-12

---

### ✅ Task 17: Integration Tests - API Endpoints
**Priority**: MEDIUM | **Estimated Time**: 5 hours

**Deliverables**:
- [ ] Create `WebApplicationFactory` test base:
  ```csharp
  public class WalletApiFactory : WebApplicationFactory<Program>
  {
      protected override void ConfigureWebHost(IWebHostBuilder builder)
      {
          builder.ConfigureServices(services =>
          {
              // Replace DbContext with test database
              services.AddDbContext<WalletDbContext>(options =>
                  options.UseInMemoryDatabase("TestDb"));
          });
      }
  }
  ```
- [ ] Test complete credential issuance flow
- [ ] Test authentication and authorization
- [ ] Test presentation request/response cycle
- [ ] Test error scenarios (401, 403, 404, 500)
- [ ] Test concurrent requests

**Acceptance Criteria**:
- All endpoints tested end-to-end
- Tests use realistic test data
- Authentication is properly tested
- Tests clean up after themselves

**Dependencies**: Tasks 5-15

---

## 🏭 Phase 5: Production Ready (Week 5)

### ✅ Task 18: Repository Pattern Implementation
**Priority**: MEDIUM | **Estimated Time**: 4 hours

**Deliverables**:
- [ ] Create interfaces in `CredVault.Core/Interfaces/`:
  ```csharp
  public interface ICredentialRepository
  {
      Task<CredentialEntity?> GetByIdAsync(Guid id);
      Task<List<CredentialEntity>> GetByUserIdAsync(string userId);
      Task<CredentialEntity> AddAsync(CredentialEntity credential);
      Task UpdateAsync(CredentialEntity credential);
      Task DeleteAsync(Guid id);
      Task<bool> ExistsAsync(Guid id);
  }
  ```
- [ ] Implement repositories in `CredVault.Infrastructure/Repositories/`
- [ ] Add specification pattern for complex queries
- [ ] Implement unit of work pattern
- [ ] Add query optimization and includes

**Acceptance Criteria**:
- All data access goes through repositories
- Queries are optimized with proper includes
- Repositories are testable with mocks
- Unit of work ensures transaction consistency

**Dependencies**: Task 2

---

### ✅ Task 19: Logging & Telemetry
**Priority**: MEDIUM | **Estimated Time**: 3 hours

**Deliverables**:
- [ ] Configure Serilog in `Program.cs`:
  ```csharp
  Log.Logger = new LoggerConfiguration()
      .ReadFrom.Configuration(builder.Configuration)
      .Enrich.FromLogContext()
      .Enrich.WithProperty("Application", "CredVault")
      .WriteTo.Console()
      .WriteTo.File("logs/credvault-.log", rollingInterval: RollingInterval.Day)
      .CreateLogger();
  
  builder.Host.UseSerilog();
  ```
- [ ] Add structured logging to controllers
- [ ] Implement correlation ID middleware
- [ ] Add performance metrics with Application Insights
- [ ] Create audit log for sensitive operations

**Acceptance Criteria**:
- All requests have correlation IDs
- Sensitive operations are audited
- Performance is tracked
- Logs are structured and searchable

**Dependencies**: Task 1

---

### ✅ Task 20: Configuration & Secrets Management
**Priority**: MEDIUM | **Estimated Time**: 2 hours

**Deliverables**:
- [ ] Set up `appsettings.json` hierarchy:
  ```json
  {
    "ConnectionStrings": {
      "DefaultConnection": "Server=...;Database=CredVault;..."
    },
    "Jwt": {
      "Issuer": "https://api.govstackwallet.gov",
      "Audience": "credvault-api",
      "ExpirationMinutes": 60
    },
    "OpenId4VCI": {
      "AuthorizationEndpoint": "...",
      "TokenEndpoint": "...",
      "CredentialEndpoint": "..."
    }
  }
  ```
- [ ] Configure user secrets for development
- [ ] Add environment-specific configurations
- [ ] Document required environment variables
- [ ] Set up Azure Key Vault integration (optional)

**Acceptance Criteria**:
- No secrets in source control
- Configuration validates on startup
- Environment-specific settings work
- Documentation is complete

**Dependencies**: Task 1

---

## 📈 Success Metrics

### Code Quality
- [ ] Code coverage > 80%
- [ ] Zero critical security vulnerabilities
- [ ] All tests passing
- [ ] Code review approval on all PRs

### API Performance
- [ ] Response time < 200ms for GET requests
- [ ] Response time < 500ms for POST requests
- [ ] Support 100 concurrent users
- [ ] 99.9% uptime

### Documentation
- [ ] Complete API documentation in Swagger
- [ ] README with setup instructions
- [ ] Architecture decision records (ADRs)
- [ ] Deployment guide

---

## 🔄 Development Workflow

### Daily Standup Questions
1. What did I complete yesterday?
2. What am I working on today?
3. What blockers do I have?

### Definition of Done (DoD)
- [ ] Code written and committed
- [ ] Unit tests written and passing
- [ ] Integration tests passing
- [ ] Code reviewed and approved
- [ ] Documentation updated
- [ ] No critical bugs
- [ ] Merged to main branch

### Pull Request Template
```markdown
## Description
Brief description of changes

## Related Tasks
- Closes #1
- Related to #2

## Type of Change
- [ ] New feature
- [ ] Bug fix
- [ ] Breaking change
- [ ] Documentation

## Testing
- [ ] Unit tests added
- [ ] Integration tests added
- [ ] Manual testing completed

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-review completed
- [ ] Documentation updated
- [ ] No new warnings
```

---

## 🚨 Risk Management

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| OpenID4VCI complexity | High | Medium | Early prototype, expert consultation |
| Cryptography errors | Critical | Low | Use established libraries, security audit |
| Performance issues | Medium | Medium | Load testing, caching strategy |
| Breaking API changes | High | Low | Versioning, deprecation policy |
| Missing requirements | Medium | Medium | Regular stakeholder reviews |

---

## 📞 Key Contacts & Resources

### Documentation References
- OpenID for Verifiable Credential Issuance: https://openid.net/specs/openid-4-verifiable-credential-issuance-1_0.html
- DIF Presentation Exchange: https://identity.foundation/presentation-exchange/
- W3C Verifiable Credentials: https://www.w3.org/TR/vc-data-model/
- .NET 8 Documentation: https://learn.microsoft.com/en-us/dotnet/

### Tools & Environments
- **Dev Environment**: localhost:5000
- **Test Environment**: https://test.govstackwallet.gov
- **Production**: https://api.govstackwallet.gov
- **CI/CD**: GitHub Actions
- **Database**: SQL Server 2022
- **Code Repository**: GitHub

---

## 🎓 Learning Resources

- [ ] OpenID4VCI specification
- [ ] Entity Framework Core best practices
- [ ] JWT authentication patterns
- [ ] Verifiable Credentials data model
- [ ] Selective disclosure techniques

---

## 📝 Notes & Decisions

### 2025-10-31: Initial Planning
- Decided on .NET 8 for long-term support
- Chose Entity Framework Core over Dapper for rapid development
- Selected OpenID4VCI as issuance protocol
- Agreed on RESTful API design over GraphQL

---

## 🔄 Version History

| Version | Date | Changes | Author |
|---------|------|---------|--------|
| 1.0 | 2025-10-31 | Initial development plan created | GitHub Copilot |

---

**End of Development Plan**

*This document is the single source of truth. Update this file as tasks progress and decisions are made.*
