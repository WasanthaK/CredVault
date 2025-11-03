namespace CredVault.Mobile.Models;

/// <summary>
/// Transaction/Activity log entry
/// </summary>
public class TransactionLogResponseDto
{
    public string? Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Action { get; set; }
    public string? CredentialId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
}

/// <summary>
/// Workflow credential request
/// </summary>
public class WorkflowCredentialRequest
{
    public required string RequestId { get; set; }
    public required string CitizenSub { get; set; }
    public required string RecordId { get; set; }
}

/// <summary>
/// Workflow credential response
/// </summary>
public class WorkflowCredentialResponse
{
    public string? CredentialId { get; set; }
    public string? VerifierUrl { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? Status { get; set; }
    public string? RequestId { get; set; }
    public string? RecordId { get; set; }
    public string? CitizenSub { get; set; }
}

/// <summary>
/// Citizen credentials response
/// </summary>
public class CitizenCredentialsResponse
{
    public string? CitizenSub { get; set; }
    public List<WorkflowCredentialResponse>? Credentials { get; set; }
    public int TotalCount { get; set; }
}

/// <summary>
/// Health check response
/// </summary>
public class GovStackHealthResponse
{
    public string? Status { get; set; }
    public string? Service { get; set; }
    public string? Version { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Environment { get; set; }
    public long Uptime { get; set; }
    public string? BuildingBlock { get; set; }
    public bool GovStackCompliant { get; set; }
    public object? Details { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Error response
/// </summary>
public class GovStackErrorResponse
{
    public string? Error { get; set; }
    public string? ErrorDescription { get; set; }
    public int? StatusCode { get; set; }
    public DateTime Timestamp { get; set; }
    public string? TraceId { get; set; }
}
