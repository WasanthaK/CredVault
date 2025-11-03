using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CredVault.Mobile.Services;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace CredVault.Mobile.ViewModels;

public partial class VerifierViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly WalletService _walletService;
    private readonly ILogger<VerifierViewModel> _logger;

    [ObservableProperty]
    private string title = "Verifier Mode";

    [ObservableProperty]
    private string? verificationRequestId;

    [ObservableProperty]
    private string? qrCodeData;

    [ObservableProperty]
    private bool showQrCodeModal;

    // Verification Result
    [ObservableProperty]
    private bool isVerified;

    [ObservableProperty]
    private string? verificationStatus;

    [ObservableProperty]
    private string? statusMessage;

    [ObservableProperty]
    private string? holderName;

    [ObservableProperty]
    private string? credentialType;

    [ObservableProperty]
    private string? issuerName;

    [ObservableProperty]
    private string? expiryDate;

    [ObservableProperty]
    private ObservableCollection<VerifiedClaimItem> verifiedClaims = new();

    public VerifierViewModel(
        INavigationService navigationService,
        WalletService walletService,
        ILogger<VerifierViewModel> logger)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ScanCredentialQrAsync()
    {
        try
        {
            _logger.LogInformation("Navigating to QR scanner for credential verification");
            
            var navigationParams = new Dictionary<string, object>
            {
                ["mode"] = "VerifierScan"
            };
            
            await _navigationService.NavigateToAsync("scanner", navigationParams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to QR scanner");
            if (Application.Current?.Windows.Count > 0)
            {
                await Application.Current.Windows[0].Page!.DisplayAlert(
                    "Error",
                    "Failed to open QR scanner",
                    "OK");
            }
        }
    }

    [RelayCommand]
    private async Task CreateManualRequestAsync()
    {
        try
        {
            _logger.LogInformation("Navigating to manual request page");
            await _navigationService.NavigateToAsync("manualrequest");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to manual request page");
        }
    }

    [RelayCommand]
    private async Task ProcessScannedDataAsync(string scannedData)
    {
        try
        {
            _logger.LogInformation("Processing scanned credential data");

            // In a real implementation, this would parse the scanned VP JWT
            // and verify signatures, expiration, revocation status, etc.
            
            // For now, simulate verification
            await Task.Delay(1500); // Simulate verification delay

            // Mock verification result - in production, call verification API
            var mockVerification = SimulateVerification(scannedData);

            IsVerified = mockVerification.IsValid;
            VerificationStatus = mockVerification.Status;
            StatusMessage = mockVerification.Message;
            HolderName = mockVerification.HolderName;
            CredentialType = mockVerification.CredentialType;
            IssuerName = mockVerification.Issuer;
            ExpiryDate = mockVerification.ExpiryDate;

            VerifiedClaims.Clear();
            foreach (var claim in mockVerification.Claims)
            {
                VerifiedClaims.Add(claim);
            }

            _logger.LogInformation("Credential verification completed: {Status}", VerificationStatus);

            // Navigate to result page
            await _navigationService.NavigateToAsync("verificationresult");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing scanned credential");
            if (Application.Current?.Windows.Count > 0)
            {
                await Application.Current.Windows[0].Page!.DisplayAlert(
                    "Error",
                    "Failed to verify credential",
                    "OK");
            }
        }
    }

    [RelayCommand]
    private async Task ScanAnotherCredentialAsync()
    {
        try
        {
            // Clear previous result
            IsVerified = false;
            VerificationStatus = null;
            StatusMessage = null;
            HolderName = null;
            CredentialType = null;
            IssuerName = null;
            ExpiryDate = null;
            VerifiedClaims.Clear();

            // Navigate back to scan page
            var navigationParams = new Dictionary<string, object>
            {
                ["mode"] = "VerifierScan"
            };
            await _navigationService.NavigateToAsync("scanner", navigationParams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to scan page");
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await _navigationService.GoBackAsync();
    }

    [RelayCommand]
    private void CloseQrCodeModal()
    {
        ShowQrCodeModal = false;
        QrCodeData = null;
    }

    private VerificationResult SimulateVerification(string scannedData)
    {
        // Simulate different verification outcomes based on data content
        var random = new Random();
        var isValid = !scannedData.Contains("revoked") && !scannedData.Contains("expired");

        if (isValid)
        {
            return new VerificationResult
            {
                IsValid = true,
                Status = "Valid",
                Message = "Credential is valid and verified",
                HolderName = "John Doe",
                CredentialType = "Driver's License",
                Issuer = "Government Authority",
                ExpiryDate = "12/31/2028",
                Claims = new List<VerifiedClaimItem>
                {
                    new VerifiedClaimItem { Name = "Full Name", ClaimValue = "John Doe", IsVerified = true },
                    new VerifiedClaimItem { Name = "Date of Birth", ClaimValue = "01/15/1990", IsVerified = true },
                    new VerifiedClaimItem { Name = "License Number", ClaimValue = "DL12345678", IsVerified = true },
                    new VerifiedClaimItem { Name = "Age Over 18", ClaimValue = "Yes", IsVerified = true },
                    new VerifiedClaimItem { Name = "Nationality", ClaimValue = "US Citizen", IsVerified = true }
                }
            };
        }
        else
        {
            return new VerificationResult
            {
                IsValid = false,
                Status = scannedData.Contains("revoked") ? "Revoked" : "Expired",
                Message = "Please ask the holder to present a different credential.",
                HolderName = "Jane Smith",
                CredentialType = "ID Card",
                Issuer = "Government Authority",
                ExpiryDate = "06/30/2023",
                Claims = new List<VerifiedClaimItem>()
            };
        }
    }

    private class VerificationResult
    {
        public bool IsValid { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string HolderName { get; set; } = string.Empty;
        public string CredentialType { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string ExpiryDate { get; set; } = string.Empty;
        public List<VerifiedClaimItem> Claims { get; set; } = new();
    }
}

public partial class VerifiedClaimItem : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string claimValue = string.Empty;

    [ObservableProperty]
    private bool isVerified;
}
