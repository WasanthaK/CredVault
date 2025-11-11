namespace CredVault.Mobile.Models;

/// <summary>
/// Credential offer details from issuer (OpenID4VCI)
/// </summary>
public class CredentialOfferDetails
{
    public required string CredentialType { get; set; }
    public required string IssuerId { get; set; }
    public required string IssuerName { get; set; }
    public string? IssuerLogoUrl { get; set; }
    public required Dictionary<string, object> Claims { get; set; }
    public string? SchemaId { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? Format { get; set; }
    public string? CredentialIdentifier { get; set; }
    public string? CNonce { get; set; }
    public string? AccessToken { get; set; }
}

/// <summary>
/// Issuer metadata for OpenID4VCI flow
/// </summary>
public class IssuerMetadata
{
    public required string IssuerId { get; set; }
    public required string IssuerName { get; set; }
    public required string AuthorizationEndpoint { get; set; }
    public required string TokenEndpoint { get; set; }
    public required string CredentialEndpoint { get; set; }
    public List<string>? SupportedCredentialTypes { get; set; }
    public Dictionary<string, object>? AdditionalMetadata { get; set; }
}

/// <summary>
/// PKCE (Proof Key for Code Exchange) parameters for OAuth
/// </summary>
public class PKCEParameters
{
    public required string CodeVerifier { get; set; }
    public required string CodeChallenge { get; set; }
    public string CodeChallengeMethod { get; set; } = "S256";
    public required string State { get; set; }
}
