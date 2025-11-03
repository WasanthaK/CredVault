namespace CredVault.Mobile.Models;

/// <summary>
/// Device transfer export request
/// </summary>
public class ExportCredentialsRequestDto
{
    public string? DevicePublicKey { get; set; }
    public string? DeviceId { get; set; }
}

/// <summary>
/// Device transfer export response
/// </summary>
public class ExportCredentialsResponseDto
{
    public string? EncryptedData { get; set; }
    public string? EncryptionKey { get; set; }
    public int CredentialCount { get; set; }
    public DateTime ExportedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? TransferId { get; set; }
}

/// <summary>
/// Device transfer import request
/// </summary>
public class ImportCredentialsRequestDto
{
    public string? EncryptedData { get; set; }
    public string? EncryptionKey { get; set; }
    public string? NewDevicePublicKey { get; set; }
    public string? DeviceId { get; set; }
}

/// <summary>
/// Device transfer import response
/// </summary>
public class ImportCredentialsResponseDto
{
    public int ImportedCredentialCount { get; set; }
    public string? DeviceId { get; set; }
    public DateTime ImportedAt { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Transfer validation request
/// </summary>
public class ValidateExportRequestDto
{
    public string? TransferId { get; set; }
    public string? EncryptionKey { get; set; }
}

/// <summary>
/// Transfer validation response
/// </summary>
public class ValidationResponseDto
{
    public bool IsValid { get; set; }
    public string? TransferId { get; set; }
    public int CredentialCount { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Message { get; set; }
}
