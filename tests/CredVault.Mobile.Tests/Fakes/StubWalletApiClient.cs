using CredVault.Mobile.Models;
using CredVault.Mobile.Services;

namespace CredVault.Mobile.Tests.Fakes;

internal sealed class StubWalletApiClient : IWalletApiClient
{
    public Task<ApiResponseDto<List<CredentialResponseDto>>> GetCredentialsAsync(CredentialQuery? query = null)
        => throw new NotImplementedException();

    public Task<ApiResponseDto<CredentialResponseDto>> GetCredentialByIdAsync(Guid credentialId)
        => throw new NotImplementedException();

    public Task<ApiResponseDto<CredentialResponseDto>> CreateCredentialAsync(CredentialRequestDto request)
        => throw new NotImplementedException();

    public Task<BooleanApiResponseDto> RevokeCredentialAsync(Guid credentialId)
        => throw new NotImplementedException();

    public Task<BooleanApiResponseDto> SuspendCredentialAsync(Guid credentialId)
        => throw new NotImplementedException();

    public Task<BooleanApiResponseDto> ReactivateCredentialAsync(Guid credentialId)
        => throw new NotImplementedException();

    public Task<ApiResponseDto<CredentialStatusResponseDto>> GetCredentialStatusAsync(Guid credentialId)
        => throw new NotImplementedException();

    public Task<ApiResponseDto<HolderResponseDto>> GetHolderAsync(Guid holderId)
        => throw new NotImplementedException();

    public Task<ApiResponseDto<HolderResponseDto>> CreateHolderAsync(HolderRegistrationDto request)
        => throw new NotImplementedException();

    public Task<ApiResponseDto<HolderResponseDto>> UpdateHolderAsync(Guid holderId, HolderUpdateDto request)
        => throw new NotImplementedException();

    public Task<ApiResponseDto<OpenID4VCICredentialResponseDto>> RequestCredentialAsync(OpenID4VCICredentialRequestDto request)
        => throw new NotImplementedException();

    public Task<ApiResponseDto<OpenID4VCIBatchCredentialResponseDto>> RequestBatchCredentialsAsync(OpenID4VCIBatchCredentialRequestDto request)
        => throw new NotImplementedException();

    public Task<ApiResponseDto<OpenID4VPAuthorizationResponseDto>> CreateAuthorizationRequestAsync(OpenID4VPAuthorizationRequestDto request)
        => throw new NotImplementedException();

    public Task<BooleanApiResponseDto> VerifyPresentationAsync(VpTokenDto vpToken)
        => throw new NotImplementedException();

    public Task<ApiResponseDto<ExportCredentialsResponseDto>> ExportCredentialsAsync(ExportCredentialsRequestDto request)
        => throw new NotImplementedException();

    public Task<ApiResponseDto<ImportCredentialsResponseDto>> ImportCredentialsAsync(ImportCredentialsRequestDto request)
        => throw new NotImplementedException();

    public Task<ApiResponseDto<ValidationResponseDto>> ValidateExportAsync(ValidateExportRequestDto request)
        => throw new NotImplementedException();

    public Task<ApiResponseDto<List<TransactionLogResponseDto>>> GetActivityLogsAsync(int page = 1, int pageSize = 20)
        => throw new NotImplementedException();

    public Task<ApiResponseDto<WorkflowCredentialResponse>> RequestWorkflowCredentialAsync(WorkflowCredentialRequest request)
        => throw new NotImplementedException();

    public Task<ApiResponseDto<CitizenCredentialsResponse>> GetCitizenCredentialsAsync(string citizenSub)
        => throw new NotImplementedException();

    public Task<GovStackHealthResponse> GetHealthAsync()
        => throw new NotImplementedException();
}
