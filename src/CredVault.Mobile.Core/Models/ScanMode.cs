namespace CredVault.Mobile.Core.Models;

/// <summary>
/// Scan mode for the QR scanner
/// </summary>
public enum ScanMode
{
    CredentialOffer,
    PresentationRequest,
    VerifierScan,
    Any
}
