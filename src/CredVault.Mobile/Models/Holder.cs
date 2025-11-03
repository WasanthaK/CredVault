namespace CredVault.Mobile.Models;

/// <summary>
/// Holder response DTO
/// </summary>
public class HolderResponseDto
{
    public string? Id { get; set; }
    public string? HolderId { get; set; }
    public string? DisplayName { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Did { get; set; }
    public string? PublicKey { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
    public int CredentialCount { get; set; }
    public Dictionary<string, object>? PublicProfile { get; set; }
}

/// <summary>
/// Holder registration request
/// </summary>
public class HolderRegistrationDto
{
    public required string HolderId { get; set; }
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfileData { get; set; }
    public string? PublicKey { get; set; }
    public string? Did { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Holder update request
/// </summary>
public class HolderUpdateDto
{
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public Dictionary<string, object>? ProfileData { get; set; }
    public string? PublicKey { get; set; }
    public string? PrivateKey { get; set; }
}
