namespace CredVault.Mobile.Models;

/// <summary>
/// Consent request DTO
/// </summary>
public class ConsentRequestDto
{
    public required string UserId { get; set; }
    public required string ResourceOwner { get; set; }
    public required string ClientId { get; set; }
    public required List<string> Scopes { get; set; }
    public string? Purpose { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Consent response DTO
/// </summary>
public class ConsentResponseDto
{
    public required string ConsentId { get; set; }
    public required string UserId { get; set; }
    public required string ResourceOwner { get; set; }
    public required string ClientId { get; set; }
    public required List<string> Scopes { get; set; }
    public required ConsentStatus Status { get; set; }
    public string? Purpose { get; set; }
    public DateTime GrantedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Consent update DTO
/// </summary>
public class ConsentUpdateDto
{
    public List<string>? Scopes { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Consent query parameters
/// </summary>
public class ConsentQueryDto
{
    public string? UserId { get; set; }
    public string? ClientId { get; set; }
    public ConsentStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Data scope definition
/// </summary>
public class DataScopeDto
{
    public required string ScopeId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required List<string> DataFields { get; set; }
    public bool IsRequired { get; set; }
    public bool IsSensitive { get; set; }
    public string? Category { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Consent verification request
/// </summary>
public class ConsentVerificationDto
{
    public required string ConsentId { get; set; }
    public required List<string> RequiredScopes { get; set; }
}

/// <summary>
/// Consent verification response
/// </summary>
public class ConsentVerificationResponseDto
{
    public required string ConsentId { get; set; }
    public bool IsValid { get; set; }
    public bool HasAllScopes { get; set; }
    public List<string>? MissingScopes { get; set; }
    public bool IsExpired { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Bulk consent revocation
/// </summary>
public class BulkConsentRevocationDto
{
    public required List<string> ConsentIds { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Bulk consent revocation response
/// </summary>
public class BulkConsentRevocationResponseDto
{
    public int TotalRequested { get; set; }
    public int SuccessfullyRevoked { get; set; }
    public int Failed { get; set; }
    public List<string>? FailedConsentIds { get; set; }
    public Dictionary<string, string>? Errors { get; set; }
}

/// <summary>
/// Consent statistics
/// </summary>
public class ConsentStatisticsDto
{
    public required string UserId { get; set; }
    public int TotalConsents { get; set; }
    public int ActiveConsents { get; set; }
    public int RevokedConsents { get; set; }
    public int ExpiredConsents { get; set; }
    public Dictionary<string, int>? ConsentsByClient { get; set; }
    public Dictionary<string, int>? ConsentsByStatus { get; set; }
}
