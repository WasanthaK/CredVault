namespace CredVault.Mobile.Models;

/// <summary>
/// OpenID4VCI credential request
/// </summary>
public class OpenID4VCICredentialRequestDto
{
    public string? Format { get; set; }
    public string? CredentialIdentifier { get; set; }
    public string? DocType { get; set; }
    public OpenID4VCIProofObject? Proof { get; set; }
    public OpenID4VCICredentialResponseEncryption? CredentialResponseEncryption { get; set; }
    public Dictionary<string, object>? CredentialSubject { get; set; }
}

/// <summary>
/// OpenID4VCI credential response
/// </summary>
public class OpenID4VCICredentialResponseDto
{
    public string? Credential { get; set; }
    public string? TransactionId { get; set; }
    public string? CNonce { get; set; }
    public int? CNonceExpiresIn { get; set; }
    public string? NotificationId { get; set; }
    public string? Error { get; set; }
    public string? ErrorDescription { get; set; }
}

/// <summary>
/// OpenID4VCI batch credential request
/// </summary>
public class OpenID4VCIBatchCredentialRequestDto
{
    public required List<OpenID4VCICredentialRequestDto> CredentialRequests { get; set; }
}

/// <summary>
/// OpenID4VCI batch credential response
/// </summary>
public class OpenID4VCIBatchCredentialResponseDto
{
    public List<OpenID4VCICredentialResponseDto>? CredentialResponses { get; set; }
    public string? CNonce { get; set; }
    public int? CNonceExpiresIn { get; set; }
}

/// <summary>
/// OpenID4VCI proof object
/// </summary>
public class OpenID4VCIProofObject
{
    public required string ProofType { get; set; }
    public string? Jwt { get; set; }
    public string? Cwt { get; set; }
    public string? LdpVp { get; set; }
}

/// <summary>
/// Credential response encryption settings
/// </summary>
public class OpenID4VCICredentialResponseEncryption
{
    public object? Jwk { get; set; }
    public string? Alg { get; set; }
    public string? Enc { get; set; }
}

/// <summary>
/// Authorization request for credential issuance flow
/// </summary>
public class AuthorizationRequestDto
{
    public required string CredentialType { get; set; }
    public required string RedirectUri { get; set; }
    public string? State { get; set; }
    public string? Scope { get; set; }
    public string? CodeChallenge { get; set; }
    public string? CodeChallengeMethod { get; set; }
}

/// <summary>
/// Authorization response with auth code
/// </summary>
public class AuthorizationResponseDto
{
    public required string AuthorizationUrl { get; set; }
    public string? AuthorizationCode { get; set; }
    public string? State { get; set; }
    public string? Error { get; set; }
    public string? ErrorDescription { get; set; }
}

/// <summary>
/// Token request for exchanging authorization code
/// </summary>
public class TokenRequestDto
{
    public required string GrantType { get; set; }
    public required string Code { get; set; }
    public required string RedirectUri { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? CodeVerifier { get; set; }
}

/// <summary>
/// Token response with access token
/// </summary>
public class TokenResponseDto
{
    public required string AccessToken { get; set; }
    public required string TokenType { get; set; }
    public int ExpiresIn { get; set; }
    public string? RefreshToken { get; set; }
    public string? CNonce { get; set; }
    public int? CNonceExpiresIn { get; set; }
}
