namespace CredVault.Mobile.Models;

/// <summary>
/// Issuer information
/// </summary>
public class Issuer
{
    public string? Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string ContactInformation { get; set; }
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? Did { get; set; }
    public string? PublicKey { get; set; }
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Issuer registration request
/// </summary>
public class IssuerRegistrationDto
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string ContactInformation { get; set; }
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? Did { get; set; }
    public string? PublicKey { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
