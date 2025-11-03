namespace CredVault.Mobile.Models;

/// <summary>
/// Credential format types
/// </summary>
public enum CredentialFormat
{
    JsonLd = 0,
    Jwt = 1,
    SdJwt = 2,
    IsoMdl = 3,
    AnonCreds = 4
}

/// <summary>
/// Credential status
/// </summary>
public enum CredentialStatus
{
    Active = 0,
    Expired = 1,
    Revoked = 2,
    Suspended = 3,
    Pending = 4,
    Invalid = 5
}

/// <summary>
/// Consent status
/// </summary>
public enum ConsentStatus
{
    Pending = 0,
    Granted = 1,
    Revoked = 2,
    Expired = 3
}
