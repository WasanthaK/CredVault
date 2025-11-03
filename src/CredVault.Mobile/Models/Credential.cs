namespace CredVault.Mobile.Models;

/// <summary>
/// Credential response DTO
/// </summary>
public class CredentialResponseDto
{
    public string? Id { get; set; }
    public string? CredentialId { get; set; }
    public CredentialFormat Format { get; set; }
    public string? Type { get; set; }
    public string? Subject { get; set; }
    public string? Issuer { get; set; }
    public string? IssuerId { get; set; }
    public string? HolderId { get; set; }
    public string? SchemaId { get; set; }
    public CredentialStatus Status { get; set; }
    public DateTime IssuanceDate { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Dictionary<string, object>? Claims { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public string? CredentialData { get; set; }
    public string? Proof { get; set; }
}

/// <summary>
/// Credential request DTO
/// </summary>
public class CredentialRequestDto
{
    public required string Type { get; set; }
    public required string Subject { get; set; }
    public required string Issuer { get; set; }
    public required string IssuerId { get; set; }
    public required string HolderId { get; set; }
    public required string SchemaId { get; set; }
    public CredentialFormat Format { get; set; }
    public DateTime IssuanceDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public required Dictionary<string, object> Claims { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Credential status response
/// </summary>
public class CredentialStatusResponseDto
{
    public CredentialStatus Status { get; set; }
    public bool IsValid { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUpdated { get; set; }
    public string? Message { get; set; }
    public string? StatusId { get; set; }
}

/// <summary>
/// Credential query request
/// </summary>
public class CredentialQuery
{
    public string? Type { get; }
    public CredentialFilters? Filters { get; set; }
    public Pagination? Pagination { get; set; }
}

/// <summary>
/// Credential filters
/// </summary>
public class CredentialFilters
{
    public string? HolderId { get; set; }
    public string? CredentialType { get; set; }
    public string? Status { get; set; }
    public DateTime? IssuedAfter { get; set; }
    public DateTime? IssuedBefore { get; set; }
    public string? Issuer { get; set; }
}

/// <summary>
/// Pagination parameters
/// </summary>
public class Pagination
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 20;
}
