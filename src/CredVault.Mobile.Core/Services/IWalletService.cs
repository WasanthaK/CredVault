using CredVault.Mobile.Core.Models;

namespace CredVault.Mobile.Core.Services;

/// <summary>
/// Service for wallet operations
/// </summary>
public interface IWalletService
{
    Task<ServiceResult<List<CredentialInfo>>> GetCredentialsAsync();
    Task<ServiceResult<CredentialInfo>> GetCredentialAsync(string credentialId);
}

/// <summary>
/// Service result wrapper
/// </summary>
public class ServiceResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Credential information
/// </summary>
public class CredentialInfo
{
    public string? Id { get; set; }
    public string? Type { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public CredentialStatus Status { get; set; }
}

/// <summary>
/// Credential status
/// </summary>
public enum CredentialStatus
{
    Active,
    Expiring,
    Expired,
    Revoked
}
