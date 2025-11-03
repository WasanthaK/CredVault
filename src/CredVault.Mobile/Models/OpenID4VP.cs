namespace CredVault.Mobile.Models;

/// <summary>
/// OpenID4VP authorization request
/// </summary>
public class OpenID4VPAuthorizationRequestDto
{
    public PresentationDefinitionDto? PresentationDefinition { get; set; }
    public string? PresentationDefinitionUri { get; set; }
    public required string ClientId { get; set; }
    public string? ClientIdScheme { get; set; }
    public object? ClientMetadata { get; set; }
    public string? ResponseMode { get; set; }
    public string? ResponseType { get; set; }
    public string? Nonce { get; set; }
    public string? Scope { get; set; }
    public string? State { get; set; }
}

/// <summary>
/// OpenID4VP authorization response
/// </summary>
public class OpenID4VPAuthorizationResponseDto
{
    public required string VpToken { get; set; }
    public PresentationSubmissionDto? PresentationSubmission { get; set; }
    public string? State { get; set; }
}

/// <summary>
/// Presentation definition
/// </summary>
public class PresentationDefinitionDto
{
    public required string Id { get; set; }
    public string? Name { get; set; }
    public string? Purpose { get; set; }
    public required List<InputDescriptorDto> InputDescriptors { get; set; }
    public List<SubmissionRequirementDto>? SubmissionRequirements { get; set; }
}

/// <summary>
/// Input descriptor
/// </summary>
public class InputDescriptorDto
{
    public required string Id { get; set; }
    public string? Name { get; set; }
    public string? Purpose { get; set; }
    public ConstraintsDto? Constraints { get; set; }
}

/// <summary>
/// Constraints
/// </summary>
public class ConstraintsDto
{
    public List<FieldConstraintDto>? Fields { get; set; }
    public LimitDisclosureDto? LimitDisclosure { get; set; }
}

/// <summary>
/// Field constraint
/// </summary>
public class FieldConstraintDto
{
    public List<string>? Path { get; set; }
    public string? Filter { get; set; }
    public string? Purpose { get; set; }
}

/// <summary>
/// Limit disclosure
/// </summary>
public class LimitDisclosureDto
{
    public int? Max { get; set; }
    public int? Min { get; set; }
}

/// <summary>
/// Submission requirement
/// </summary>
public class SubmissionRequirementDto
{
    public string? Name { get; set; }
    public string? Purpose { get; set; }
    public required string Rule { get; set; }
    public int? Count { get; set; }
    public int? Min { get; set; }
    public int? Max { get; set; }
    public List<SubmissionRequirementDto>? From { get; set; }
    public List<string>? FromNested { get; set; }
}

/// <summary>
/// Presentation submission
/// </summary>
public class PresentationSubmissionDto
{
    public required string Id { get; set; }
    public required string DefinitionId { get; set; }
    public required List<DescriptorMapDto> DescriptorMap { get; set; }
}

/// <summary>
/// Descriptor map
/// </summary>
public class DescriptorMapDto
{
    public required string Id { get; set; }
    public required string Format { get; set; }
    public required string Path { get; set; }
    public NestedPathDto? PathNested { get; set; }
}

/// <summary>
/// Nested path
/// </summary>
public class NestedPathDto
{
    public required string Format { get; set; }
    public required string Path { get; set; }
}

/// <summary>
/// VP Token DTO
/// </summary>
public class VpTokenDto
{
    public required object VpToken { get; set; }
    public PresentationSubmissionDto? PresentationSubmission { get; set; }
}

/// <summary>
/// Verification result from verifier
/// </summary>
public class VerificationResultDto
{
    public bool IsValid { get; set; }
    public string? VerificationId { get; set; }
    public string? Status { get; set; }
    public Dictionary<string, object>? VerifiedClaims { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime? VerifiedAt { get; set; }
}

/// <summary>
/// Verify credential request
/// </summary>
public class VerifyCredentialRequestDto
{
    public required string Credential { get; set; }
    public string? CredentialType { get; set; }
    public Dictionary<string, object>? Options { get; set; }
}

/// <summary>
/// Presentation request from holder to verifier
/// </summary>
public class PresentationRequestDto
{
    public string? VerifierId { get; set; }
    public List<string>? RequestedClaims { get; set; }
    public string? Purpose { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Presentation response after successful credential presentation
/// </summary>
public class PresentationResponseDto
{
    public required string PresentationId { get; set; }
    public string? VpToken { get; set; }
    public string? Status { get; set; }
    public DateTime? PresentedAt { get; set; }
}
