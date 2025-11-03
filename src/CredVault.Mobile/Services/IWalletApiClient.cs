using CredVault.Mobile.Models;
using Refit;

namespace CredVault.Mobile.Services;

public interface IWalletApiClient
{
    // Credential endpoints
    [Get("/api/v1/wallet/Credential")]
    Task<ApiResponseDto<List<CredentialResponseDto>>> GetCredentialsAsync([Query] CredentialQuery? query = null);
    
    [Get("/api/v1/wallet/Credential/{credentialId}")]
    Task<ApiResponseDto<CredentialResponseDto>> GetCredentialByIdAsync(Guid credentialId);
    
    [Post("/api/v1/wallet/Credential")]
    Task<ApiResponseDto<CredentialResponseDto>> CreateCredentialAsync([Body] CredentialRequestDto request);
    
    [Post("/api/v1/wallet/Credential/{credentialId}/revoke")]
    Task<BooleanApiResponseDto> RevokeCredentialAsync(Guid credentialId);
    
    [Post("/api/v1/wallet/Credential/{credentialId}/suspend")]
    Task<BooleanApiResponseDto> SuspendCredentialAsync(Guid credentialId);
    
    [Post("/api/v1/wallet/Credential/{credentialId}/reactivate")]
    Task<BooleanApiResponseDto> ReactivateCredentialAsync(Guid credentialId);
    
    [Get("/api/v1/wallet/Credential/{credentialId}/status")]
    Task<ApiResponseDto<CredentialStatusResponseDto>> GetCredentialStatusAsync(Guid credentialId);
    
    // Holder endpoints  
    [Get("/api/v1/wallet/Holder/{holderId}")]
    Task<ApiResponseDto<HolderResponseDto>> GetHolderAsync(Guid holderId);
    
    [Post("/api/v1/wallet/Holder")]
    Task<ApiResponseDto<HolderResponseDto>> CreateHolderAsync([Body] HolderRegistrationDto request);
    
    [Put("/api/v1/wallet/Holder/{holderId}")]
    Task<ApiResponseDto<HolderResponseDto>> UpdateHolderAsync(Guid holderId, [Body] HolderUpdateDto request);
    
    [Get("/api/v1/wallet/Credential/holder/{holderId}")]
    Task<ApiResponseDto<List<CredentialResponseDto>>> GetHolderCredentialsAsync(Guid holderId);
    
    [Get("/api/v1/wallet/Holder/credential/{credentialId}")]
    Task<ApiResponseDto<CredentialResponseDto>> GetHolderCredentialAsync(Guid credentialId);
    
    // OpenID4VCI - Credential Issuance (corrected paths)
    [Post("/api/v1/wallet/credential")]
    Task<ApiResponseDto<OpenID4VCICredentialResponseDto>> RequestCredentialAsync([Body] OpenID4VCICredentialRequestDto request);
    
    [Post("/api/v1/wallet/batch_credential")]
    Task<ApiResponseDto<OpenID4VCIBatchCredentialResponseDto>> RequestBatchCredentialsAsync([Body] OpenID4VCIBatchCredentialRequestDto request);
    
    // Authorization endpoints for credential issuance
    [Post("/api/v1/wallet/Authorization/authorize")]
    Task<ApiResponseDto<AuthorizationResponseDto>> InitiateCredentialIssuanceAsync([Body] AuthorizationRequestDto request);
    
    [Post("/api/v1/wallet/Authorization/token")]
    Task<ApiResponseDto<TokenResponseDto>> ExchangeAuthorizationCodeAsync([Body] TokenRequestDto request);
    
    // OpenID4VP - Verification and Presentation (corrected paths)
    [Post("/api/v1/wallet/Verifier/verify-presentation")]
    Task<ApiResponseDto<VerificationResultDto>> VerifyPresentationAsync([Body] VpTokenDto vpToken);
    
    [Post("/api/v1/wallet/Verifier/verify")]
    Task<ApiResponseDto<VerificationResultDto>> VerifyCredentialAsync([Body] VerifyCredentialRequestDto request);
    
    [Post("/api/v1/wallet/Holder/present/{credentialId}")]
    Task<ApiResponseDto<PresentationResponseDto>> PresentCredentialAsync(Guid credentialId, [Body] PresentationRequestDto request);
    
    // Device Transfer endpoints (Backup/Restore)
    [Post("/api/v1/wallet/DeviceTransfer/export")]
    Task<ApiResponseDto<ExportCredentialsResponseDto>> ExportCredentialsAsync([Body] ExportCredentialsRequestDto request);
    
    [Post("/api/v1/wallet/DeviceTransfer/import")]
    Task<ApiResponseDto<ImportCredentialsResponseDto>> ImportCredentialsAsync([Body] ImportCredentialsRequestDto request);
    
    [Post("/api/v1/wallet/DeviceTransfer/validate-export")]
    Task<ApiResponseDto<ValidationResponseDto>> ValidateExportAsync([Body] ValidateExportRequestDto request);
    
    // Activity Log endpoints (corrected paths)
    [Get("/api/v1/wallet/Wallet/logs")]
    Task<ApiResponseDto<List<TransactionLogResponseDto>>> GetActivityLogsAsync([Query] int page = 1, [Query] int pageSize = 20);
    
    [Get("/api/v1/wallet/Wallet/logs/count")]
    Task<ApiResponseDto<int>> GetActivityLogsCountAsync();
    
    [Get("/api/v1/wallet/Wallet/credentials/{credentialId}/logs")]
    Task<ApiResponseDto<List<TransactionLogResponseDto>>> GetCredentialLogsAsync(Guid credentialId);
    
    // Workflow - Citizen Credentials (corrected paths)
    [Post("/api/v1/wallet/Workflow/issue")]
    Task<ApiResponseDto<WorkflowCredentialResponse>> IssueWorkflowCredentialAsync([Body] WorkflowCredentialRequest request);
    
    [Get("/api/v1/wallet/Workflow/citizens/{citizenSub}/credentials")]
    Task<ApiResponseDto<CitizenCredentialsResponse>> GetCitizenCredentialsAsync(string citizenSub);
    
    [Get("/api/v1/wallet/Workflow/credentials/{credentialId}")]
    Task<ApiResponseDto<CredentialResponseDto>> GetWorkflowCredentialAsync(Guid credentialId);
    
    // Health endpoint
    [Get("/api/v1/wallet/health")]
    Task<GovStackHealthResponse> GetHealthAsync();
}
