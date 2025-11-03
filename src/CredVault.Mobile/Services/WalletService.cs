using CredVault.Mobile.Models;
using Microsoft.Extensions.Logging;
using Refit;
using System.Net;

namespace CredVault.Mobile.Services;

public class WalletService
{
    private readonly IWalletApiClient _apiClient;
    private readonly ILogger<WalletService> _logger;

    public WalletService(IWalletApiClient apiClient, ILogger<WalletService> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Credential Operations

    /// <summary>
    /// Get all credentials with optional filtering
    /// </summary>
    public async Task<ServiceResult<List<CredentialResponseDto>>> GetCredentialsAsync(CredentialQuery? query = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching credentials with query: {@Query}", query);
            var response = await _apiClient.GetCredentialsAsync(query);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully fetched {Count} credentials", response.Data.Count);
                return ServiceResult<List<CredentialResponseDto>>.Success(response.Data);
            }

            _logger.LogWarning("Failed to fetch credentials: {Message}", response.Message);
            return ServiceResult<List<CredentialResponseDto>>.Failure(response.Message ?? "Failed to fetch credentials", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error fetching credentials: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<List<CredentialResponseDto>>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching credentials");
            return ServiceResult<List<CredentialResponseDto>>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Get a specific credential by ID
    /// </summary>
    public async Task<ServiceResult<CredentialResponseDto>> GetCredentialByIdAsync(Guid credentialId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching credential: {CredentialId}", credentialId);
            var response = await _apiClient.GetCredentialByIdAsync(credentialId);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully fetched credential: {CredentialId}", credentialId);
                return ServiceResult<CredentialResponseDto>.Success(response.Data);
            }

            _logger.LogWarning("Failed to fetch credential {CredentialId}: {Message}", credentialId, response.Message);
            return ServiceResult<CredentialResponseDto>.Failure(response.Message ?? "Credential not found", response.Errors?.ToArray());
        }
        catch (ApiException apiEx) when (apiEx.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Credential not found: {CredentialId}", credentialId);
            return ServiceResult<CredentialResponseDto>.Failure("Credential not found");
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error fetching credential {CredentialId}: {StatusCode}", credentialId, apiEx.StatusCode);
            return ServiceResult<CredentialResponseDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching credential {CredentialId}", credentialId);
            return ServiceResult<CredentialResponseDto>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Create a new credential
    /// </summary>
    public async Task<ServiceResult<CredentialResponseDto>> CreateCredentialAsync(CredentialRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<CredentialResponseDto>.Failure("Credential request cannot be null");

            _logger.LogInformation("Creating credential: {Type}", request.Type);
            var response = await _apiClient.CreateCredentialAsync(request);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully created credential: {CredentialId}", response.Data.CredentialId);
                return ServiceResult<CredentialResponseDto>.Success(response.Data);
            }

            _logger.LogWarning("Failed to create credential: {Message}", response.Message);
            return ServiceResult<CredentialResponseDto>.Failure(response.Message ?? "Failed to create credential", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error creating credential: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<CredentialResponseDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating credential");
            return ServiceResult<CredentialResponseDto>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Revoke a credential
    /// </summary>
    public async Task<ServiceResult<bool>> RevokeCredentialAsync(Guid credentialId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Revoking credential: {CredentialId}", credentialId);
            var response = await _apiClient.RevokeCredentialAsync(credentialId);
            
            if (response.Success)
            {
                _logger.LogInformation("Successfully revoked credential: {CredentialId}", credentialId);
                return ServiceResult<bool>.Success(response.Data);
            }

            _logger.LogWarning("Failed to revoke credential {CredentialId}: {Message}", credentialId, response.Message);
            return ServiceResult<bool>.Failure(response.Message ?? "Failed to revoke credential", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error revoking credential {CredentialId}: {StatusCode}", credentialId, apiEx.StatusCode);
            return ServiceResult<bool>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error revoking credential {CredentialId}", credentialId);
            return ServiceResult<bool>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Suspend a credential
    /// </summary>
    public async Task<ServiceResult<bool>> SuspendCredentialAsync(Guid credentialId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Suspending credential: {CredentialId}", credentialId);
            var response = await _apiClient.SuspendCredentialAsync(credentialId);
            
            if (response.Success)
            {
                _logger.LogInformation("Successfully suspended credential: {CredentialId}", credentialId);
                return ServiceResult<bool>.Success(response.Data);
            }

            _logger.LogWarning("Failed to suspend credential {CredentialId}: {Message}", credentialId, response.Message);
            return ServiceResult<bool>.Failure(response.Message ?? "Failed to suspend credential", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error suspending credential {CredentialId}: {StatusCode}", credentialId, apiEx.StatusCode);
            return ServiceResult<bool>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error suspending credential {CredentialId}", credentialId);
            return ServiceResult<bool>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Reactivate a suspended credential
    /// </summary>
    public async Task<ServiceResult<bool>> ReactivateCredentialAsync(Guid credentialId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Reactivating credential: {CredentialId}", credentialId);
            var response = await _apiClient.ReactivateCredentialAsync(credentialId);
            
            if (response.Success)
            {
                _logger.LogInformation("Successfully reactivated credential: {CredentialId}", credentialId);
                return ServiceResult<bool>.Success(response.Data);
            }

            _logger.LogWarning("Failed to reactivate credential {CredentialId}: {Message}", credentialId, response.Message);
            return ServiceResult<bool>.Failure(response.Message ?? "Failed to reactivate credential", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error reactivating credential {CredentialId}: {StatusCode}", credentialId, apiEx.StatusCode);
            return ServiceResult<bool>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error reactivating credential {CredentialId}", credentialId);
            return ServiceResult<bool>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Get credential status
    /// </summary>
    public async Task<ServiceResult<CredentialStatusResponseDto>> GetCredentialStatusAsync(Guid credentialId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching credential status: {CredentialId}", credentialId);
            var response = await _apiClient.GetCredentialStatusAsync(credentialId);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully fetched status for credential: {CredentialId}", credentialId);
                return ServiceResult<CredentialStatusResponseDto>.Success(response.Data);
            }

            _logger.LogWarning("Failed to fetch status for credential {CredentialId}: {Message}", credentialId, response.Message);
            return ServiceResult<CredentialStatusResponseDto>.Failure(response.Message ?? "Failed to fetch credential status", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error fetching credential status {CredentialId}: {StatusCode}", credentialId, apiEx.StatusCode);
            return ServiceResult<CredentialStatusResponseDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching credential status {CredentialId}", credentialId);
            return ServiceResult<CredentialStatusResponseDto>.Failure("An unexpected error occurred");
        }
    }

    #endregion

    #region Holder Operations

    /// <summary>
    /// Get holder profile by ID
    /// </summary>
    public async Task<ServiceResult<HolderResponseDto>> GetHolderAsync(Guid holderId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching holder: {HolderId}", holderId);
            var response = await _apiClient.GetHolderAsync(holderId);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully fetched holder: {HolderId}", holderId);
                return ServiceResult<HolderResponseDto>.Success(response.Data);
            }

            _logger.LogWarning("Failed to fetch holder {HolderId}: {Message}", holderId, response.Message);
            return ServiceResult<HolderResponseDto>.Failure(response.Message ?? "Holder not found", response.Errors?.ToArray());
        }
        catch (ApiException apiEx) when (apiEx.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Holder not found: {HolderId}", holderId);
            return ServiceResult<HolderResponseDto>.Failure("Holder not found");
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error fetching holder {HolderId}: {StatusCode}", holderId, apiEx.StatusCode);
            return ServiceResult<HolderResponseDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching holder {HolderId}", holderId);
            return ServiceResult<HolderResponseDto>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Register a new holder
    /// </summary>
    public async Task<ServiceResult<HolderResponseDto>> CreateHolderAsync(HolderRegistrationDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<HolderResponseDto>.Failure("Holder registration request cannot be null");

            _logger.LogInformation("Creating holder: {HolderId}", request.HolderId);
            var response = await _apiClient.CreateHolderAsync(request);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully created holder: {HolderId}", response.Data.HolderId);
                return ServiceResult<HolderResponseDto>.Success(response.Data);
            }

            _logger.LogWarning("Failed to create holder: {Message}", response.Message);
            return ServiceResult<HolderResponseDto>.Failure(response.Message ?? "Failed to create holder", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error creating holder: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<HolderResponseDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating holder");
            return ServiceResult<HolderResponseDto>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Update holder profile
    /// </summary>
    public async Task<ServiceResult<HolderResponseDto>> UpdateHolderAsync(Guid holderId, HolderUpdateDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<HolderResponseDto>.Failure("Holder update request cannot be null");

            _logger.LogInformation("Updating holder: {HolderId}", holderId);
            var response = await _apiClient.UpdateHolderAsync(holderId, request);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully updated holder: {HolderId}", holderId);
                return ServiceResult<HolderResponseDto>.Success(response.Data);
            }

            _logger.LogWarning("Failed to update holder {HolderId}: {Message}", holderId, response.Message);
            return ServiceResult<HolderResponseDto>.Failure(response.Message ?? "Failed to update holder", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error updating holder {HolderId}: {StatusCode}", holderId, apiEx.StatusCode);
            return ServiceResult<HolderResponseDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating holder {HolderId}", holderId);
            return ServiceResult<HolderResponseDto>.Failure("An unexpected error occurred");
        }
    }

    #endregion

    #region OpenID4VCI Operations

    /// <summary>
    /// Request a credential using OpenID4VCI protocol
    /// </summary>
    public async Task<ServiceResult<OpenID4VCICredentialResponseDto>> RequestCredentialAsync(OpenID4VCICredentialRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<OpenID4VCICredentialResponseDto>.Failure("Credential request cannot be null");

            _logger.LogInformation("Requesting credential via OpenID4VCI: {Format}", request.Format);
            var response = await _apiClient.RequestCredentialAsync(request);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully requested credential: {TransactionId}", response.Data.TransactionId);
                return ServiceResult<OpenID4VCICredentialResponseDto>.Success(response.Data);
            }

            _logger.LogWarning("Failed to request credential: {Message}", response.Message);
            return ServiceResult<OpenID4VCICredentialResponseDto>.Failure(response.Message ?? "Failed to request credential", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error requesting credential: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<OpenID4VCICredentialResponseDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error requesting credential");
            return ServiceResult<OpenID4VCICredentialResponseDto>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Request multiple credentials in batch using OpenID4VCI protocol
    /// </summary>
    public async Task<ServiceResult<OpenID4VCIBatchCredentialResponseDto>> RequestBatchCredentialsAsync(OpenID4VCIBatchCredentialRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<OpenID4VCIBatchCredentialResponseDto>.Failure("Batch credential request cannot be null");

            _logger.LogInformation("Requesting {Count} credentials via OpenID4VCI batch", request.CredentialRequests?.Count ?? 0);
            var response = await _apiClient.RequestBatchCredentialsAsync(request);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully requested batch credentials: {Count} responses", response.Data.CredentialResponses?.Count ?? 0);
                return ServiceResult<OpenID4VCIBatchCredentialResponseDto>.Success(response.Data);
            }

            _logger.LogWarning("Failed to request batch credentials: {Message}", response.Message);
            return ServiceResult<OpenID4VCIBatchCredentialResponseDto>.Failure(response.Message ?? "Failed to request batch credentials", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error requesting batch credentials: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<OpenID4VCIBatchCredentialResponseDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error requesting batch credentials");
            return ServiceResult<OpenID4VCIBatchCredentialResponseDto>.Failure("An unexpected error occurred");
        }
    }

    #endregion

    #region OpenID4VP Operations

    /// <summary>
    /// Create an authorization request for presentation
    /// </summary>
    public async Task<ServiceResult<OpenID4VPAuthorizationResponseDto>> CreateAuthorizationRequestAsync(OpenID4VPAuthorizationRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<OpenID4VPAuthorizationResponseDto>.Failure("Authorization request cannot be null");

            _logger.LogInformation("Creating OpenID4VP authorization request: {ClientId}", request.ClientId);
            // TODO: Implement OpenID4VP authorization request endpoint in backend
            _logger.LogWarning("OpenID4VP authorization request not yet implemented in API");
            return ServiceResult<OpenID4VPAuthorizationResponseDto>.Failure("OpenID4VP authorization not yet implemented");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating authorization request");
            return ServiceResult<OpenID4VPAuthorizationResponseDto>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Verify a presentation (VP token)
    /// </summary>
    public async Task<ServiceResult<bool>> VerifyPresentationAsync(VpTokenDto vpToken, CancellationToken cancellationToken = default)
    {
        try
        {
            if (vpToken == null)
                return ServiceResult<bool>.Failure("VP token cannot be null");

            _logger.LogInformation("Verifying presentation");
            var response = await _apiClient.VerifyPresentationAsync(vpToken);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully verified presentation: {Result}", response.Data.IsValid);
                return ServiceResult<bool>.Success(response.Data.IsValid);
            }

            _logger.LogWarning("Failed to verify presentation: {Message}", response.Message);
            return ServiceResult<bool>.Failure(response.Message ?? "Failed to verify presentation", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error verifying presentation: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<bool>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error verifying presentation");
            return ServiceResult<bool>.Failure("An unexpected error occurred");
        }
    }

    #endregion

    #region Device Transfer Operations

    /// <summary>
    /// Export credentials for device transfer
    /// </summary>
    public async Task<ServiceResult<ExportCredentialsResponseDto>> ExportCredentialsAsync(ExportCredentialsRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<ExportCredentialsResponseDto>.Failure("Export request cannot be null");

            _logger.LogInformation("Exporting credentials for device: {DeviceId}", request.DeviceId);
            var response = await _apiClient.ExportCredentialsAsync(request);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully exported {Count} credentials", response.Data.CredentialCount);
                return ServiceResult<ExportCredentialsResponseDto>.Success(response.Data);
            }

            _logger.LogWarning("Failed to export credentials: {Message}", response.Message);
            return ServiceResult<ExportCredentialsResponseDto>.Failure(response.Message ?? "Failed to export credentials", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error exporting credentials: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<ExportCredentialsResponseDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error exporting credentials");
            return ServiceResult<ExportCredentialsResponseDto>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Import credentials from device transfer
    /// </summary>
    public async Task<ServiceResult<ImportCredentialsResponseDto>> ImportCredentialsAsync(ImportCredentialsRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<ImportCredentialsResponseDto>.Failure("Import request cannot be null");

            _logger.LogInformation("Importing credentials to device: {DeviceId}", request.DeviceId);
            var response = await _apiClient.ImportCredentialsAsync(request);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully imported {Count} credentials", response.Data.ImportedCredentialCount);
                return ServiceResult<ImportCredentialsResponseDto>.Success(response.Data);
            }

            _logger.LogWarning("Failed to import credentials: {Message}", response.Message);
            return ServiceResult<ImportCredentialsResponseDto>.Failure(response.Message ?? "Failed to import credentials", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error importing credentials: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<ImportCredentialsResponseDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error importing credentials");
            return ServiceResult<ImportCredentialsResponseDto>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Validate an export package
    /// </summary>
    public async Task<ServiceResult<ValidationResponseDto>> ValidateExportAsync(ValidateExportRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<ValidationResponseDto>.Failure("Validation request cannot be null");

            _logger.LogInformation("Validating export: {TransferId}", request.TransferId);
            var response = await _apiClient.ValidateExportAsync(request);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Export validation result: {IsValid}", response.Data.IsValid);
                return ServiceResult<ValidationResponseDto>.Success(response.Data);
            }

            _logger.LogWarning("Failed to validate export: {Message}", response.Message);
            return ServiceResult<ValidationResponseDto>.Failure(response.Message ?? "Failed to validate export", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error validating export: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<ValidationResponseDto>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error validating export");
            return ServiceResult<ValidationResponseDto>.Failure("An unexpected error occurred");
        }
    }

    #endregion

    #region Activity Log Operations

    /// <summary>
    /// Get activity logs with pagination
    /// </summary>
    public async Task<ServiceResult<List<TransactionLogResponseDto>>> GetActivityLogsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching activity logs: Page {Page}, Size {PageSize}", page, pageSize);
            var response = await _apiClient.GetActivityLogsAsync(page, pageSize);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully fetched {Count} activity logs", response.Data.Count);
                return ServiceResult<List<TransactionLogResponseDto>>.Success(response.Data);
            }

            _logger.LogWarning("Failed to fetch activity logs: {Message}", response.Message);
            return ServiceResult<List<TransactionLogResponseDto>>.Failure(response.Message ?? "Failed to fetch activity logs", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error fetching activity logs: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<List<TransactionLogResponseDto>>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching activity logs");
            return ServiceResult<List<TransactionLogResponseDto>>.Failure("An unexpected error occurred");
        }
    }

    #endregion

    #region Workflow Operations

    /// <summary>
    /// Request a credential through workflow
    /// </summary>
    public async Task<ServiceResult<WorkflowCredentialResponse>> RequestWorkflowCredentialAsync(WorkflowCredentialRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ServiceResult<WorkflowCredentialResponse>.Failure("Workflow request cannot be null");

            _logger.LogInformation("Requesting workflow credential: {RequestId}", request.RequestId);
            var response = await _apiClient.IssueWorkflowCredentialAsync(request);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully requested workflow credential: {CredentialId}", response.Data.CredentialId);
                return ServiceResult<WorkflowCredentialResponse>.Success(response.Data);
            }

            _logger.LogWarning("Failed to request workflow credential: {Message}", response.Message);
            return ServiceResult<WorkflowCredentialResponse>.Failure(response.Message ?? "Failed to request workflow credential", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error requesting workflow credential: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<WorkflowCredentialResponse>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error requesting workflow credential");
            return ServiceResult<WorkflowCredentialResponse>.Failure("An unexpected error occurred");
        }
    }

    /// <summary>
    /// Get all credentials for a citizen
    /// </summary>
    public async Task<ServiceResult<CitizenCredentialsResponse>> GetCitizenCredentialsAsync(string citizenSub, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(citizenSub))
                return ServiceResult<CitizenCredentialsResponse>.Failure("Citizen subject cannot be empty");

            _logger.LogInformation("Fetching credentials for citizen: {CitizenSub}", citizenSub);
            var response = await _apiClient.GetCitizenCredentialsAsync(citizenSub);
            
            if (response.Success && response.Data != null)
            {
                _logger.LogInformation("Successfully fetched {Count} credentials for citizen", response.Data.TotalCount);
                return ServiceResult<CitizenCredentialsResponse>.Success(response.Data);
            }

            _logger.LogWarning("Failed to fetch citizen credentials: {Message}", response.Message);
            return ServiceResult<CitizenCredentialsResponse>.Failure(response.Message ?? "Failed to fetch citizen credentials", response.Errors?.ToArray());
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error fetching citizen credentials: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<CitizenCredentialsResponse>.Failure($"API error: {apiEx.Message}", GetErrorDetails(apiEx));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching citizen credentials");
            return ServiceResult<CitizenCredentialsResponse>.Failure("An unexpected error occurred");
        }
    }

    #endregion

    #region Health Check

    /// <summary>
    /// Check API health status
    /// </summary>
    public async Task<ServiceResult<GovStackHealthResponse>> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Checking API health");
            var response = await _apiClient.GetHealthAsync();
            
            if (response != null)
            {
                _logger.LogInformation("API health status: {Status}", response.Status);
                return ServiceResult<GovStackHealthResponse>.Success(response);
            }

            _logger.LogWarning("Failed to get health status");
            return ServiceResult<GovStackHealthResponse>.Failure("Failed to get health status");
        }
        catch (ApiException apiEx)
        {
            _logger.LogError(apiEx, "API error checking health: {StatusCode}", apiEx.StatusCode);
            return ServiceResult<GovStackHealthResponse>.Failure($"API error: {apiEx.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error checking health");
            return ServiceResult<GovStackHealthResponse>.Failure("An unexpected error occurred");
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Extract error details from API exception
    /// </summary>
    private string[]? GetErrorDetails(ApiException apiEx)
    {
        if (string.IsNullOrWhiteSpace(apiEx.Content))
            return null;

        try
        {
            // Try to parse error response
            var errorResponse = System.Text.Json.JsonSerializer.Deserialize<GovStackErrorResponse>(apiEx.Content);
            if (errorResponse != null && !string.IsNullOrWhiteSpace(errorResponse.ErrorDescription))
            {
                return new[] { errorResponse.ErrorDescription };
            }
        }
        catch
        {
            // If parsing fails, return the raw content
            return new[] { apiEx.Content };
        }

        return null;
    }

    #endregion
}

/// <summary>
/// Service result wrapper for consistent error handling
/// </summary>
public class ServiceResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string[]? ErrorDetails { get; set; }

    public static ServiceResult<T> Success(T data)
    {
        return new ServiceResult<T>
        {
            IsSuccess = true,
            Data = data
        };
    }

    public static ServiceResult<T> Failure(string errorMessage, string[]? errorDetails = null)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ErrorDetails = errorDetails
        };
    }
}
