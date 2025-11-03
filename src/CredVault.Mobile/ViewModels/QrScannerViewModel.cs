using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CredVault.Mobile.Services;
using Microsoft.Extensions.Logging;
using ZXing.Net.Maui;

namespace CredVault.Mobile.ViewModels;

public partial class QrScannerViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;
    private readonly ILogger<QrScannerViewModel> _logger;

    [ObservableProperty]
    private bool isScanning = true;

    [ObservableProperty]
    private bool isTorchOn;

    [ObservableProperty]
    private string scanInstruction = "Position the QR code inside the frame";

    [ObservableProperty]
    private string? lastScannedData;

    [ObservableProperty]
    private bool hasScannedData;

    [ObservableProperty]
    private ScanMode currentScanMode = ScanMode.CredentialOffer;

    public VerifierViewModel? VerifierViewModel { get; set; }

    public QrScannerViewModel(
        INavigationService navigationService,
        ILogger<QrScannerViewModel> logger)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Title = "Scan QR Code";
    }

    public void SetScanMode(ScanMode mode)
    {
        CurrentScanMode = mode;
        ScanInstruction = mode switch
        {
            ScanMode.CredentialOffer => "Scan credential offer QR code",
            ScanMode.PresentationRequest => "Scan presentation request QR code",
            ScanMode.VerifierScan => "Scan holder's credential QR code",
            _ => "Position the QR code inside the frame"
        };
    }

    [RelayCommand]
    private void BarcodeDetected(BarcodeDetectionEventArgs args)
    {
        if (!IsScanning || args.Results.Length == 0)
            return;

        var barcode = args.Results[0];
        var qrData = barcode.Value;

        if (string.IsNullOrWhiteSpace(qrData) || qrData == LastScannedData)
            return;

        LastScannedData = qrData;
        HasScannedData = true;

        // Stop scanning temporarily to process
        IsScanning = false;

        _logger.LogInformation("QR Code detected: {Data}", qrData.Length > 100 ? qrData.Substring(0, 100) + "..." : qrData);

        // Process the scanned data
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await ProcessScannedDataAsync(qrData);
        });
    }

    private async Task ProcessScannedDataAsync(string qrData)
    {
        try
        {
            IsBusy = true;

            // If in verifier mode, handle credential verification
            if (CurrentScanMode == ScanMode.VerifierScan)
            {
                _logger.LogInformation("Processing credential for verification");
                
                if (VerifierViewModel != null)
                {
                    await VerifierViewModel.ProcessScannedDataCommand.ExecuteAsync(qrData);
                }
                else
                {
                    await ShowAlertAsync("Error", "Verifier not initialized", "OK");
                    IsScanning = true;
                    HasScannedData = false;
                }
                return;
            }

            // Determine what type of QR code was scanned
            if (IsCredentialOffer(qrData))
            {
                _logger.LogInformation("Detected credential offer QR code");
                await HandleCredentialOfferAsync(qrData);
            }
            else if (IsPresentationRequest(qrData))
            {
                _logger.LogInformation("Detected presentation request QR code");
                await HandlePresentationRequestAsync(qrData);
            }
            else if (IsDeepLink(qrData))
            {
                _logger.LogInformation("Detected deep link QR code");
                await HandleDeepLinkAsync(qrData);
            }
            else
            {
                _logger.LogWarning("Unknown QR code format");
                await ShowAlertAsync("Unknown QR Code", "This QR code is not a recognized credential offer or presentation request.", "OK");
                
                // Resume scanning
                await Task.Delay(1000);
                IsScanning = true;
                HasScannedData = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing scanned QR code");
            await ShowAlertAsync("Error", "Failed to process the QR code. Please try again.", "OK");
            
            // Resume scanning
            await Task.Delay(1000);
            IsScanning = true;
            HasScannedData = false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool IsCredentialOffer(string data)
    {
        // OpenID4VCI credential offer formats:
        // - openid-credential-offer://
        // - openid-credential-offer:/?credential_offer=...
        // - https://issuer.example.com/?credential_offer=...
        // - credvault://credential-offer?...
        
        return data.StartsWith("openid-credential-offer", StringComparison.OrdinalIgnoreCase) ||
               data.Contains("credential_offer=", StringComparison.OrdinalIgnoreCase) ||
               data.Contains("credential-offer", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsPresentationRequest(string data)
    {
        // OpenID4VP presentation request formats:
        // - openid4vp://
        // - openid-vc://
        // - https://verifier.example.com/?presentation_definition=...
        // - credvault://presentation-request?...
        
        return data.StartsWith("openid4vp", StringComparison.OrdinalIgnoreCase) ||
               data.StartsWith("openid-vc", StringComparison.OrdinalIgnoreCase) ||
               data.Contains("presentation_definition", StringComparison.OrdinalIgnoreCase) ||
               data.Contains("presentation-request", StringComparison.OrdinalIgnoreCase) ||
               data.Contains("vp_token", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsDeepLink(string data)
    {
        return data.StartsWith("credvault://", StringComparison.OrdinalIgnoreCase) ||
               data.StartsWith("https://credvault.app/", StringComparison.OrdinalIgnoreCase);
    }

    private async Task HandleCredentialOfferAsync(string offerData)
    {
        try
        {
            _logger.LogInformation("Processing credential offer");
            
            // Parse the credential offer
            // In a real implementation, this would parse the OpenID4VCI credential offer
            // and extract issuer information, credential types, etc.
            
            var confirm = await ShowConfirmAsync(
                "Credential Offer",
                "A credential issuer wants to issue a credential to you. Do you want to proceed?",
                "Accept",
                "Decline");

            if (confirm)
            {
                // Navigate to the add credential flow with the offer data
                var navigationParams = new Dictionary<string, object>
                {
                    ["credentialOffer"] = offerData
                };
                
                await _navigationService.NavigateToAsync("addcredential", navigationParams);
            }
            else
            {
                // User declined, resume scanning
                await Task.Delay(500);
                IsScanning = true;
                HasScannedData = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling credential offer");
            throw;
        }
    }

    private async Task HandlePresentationRequestAsync(string requestData)
    {
        try
        {
            _logger.LogInformation("Processing presentation request");
            
            // Parse the presentation request
            // In a real implementation, this would parse the OpenID4VP presentation definition
            // and determine which credentials are being requested
            
            var confirm = await ShowConfirmAsync(
                "Presentation Request",
                "A verifier is requesting to verify your credentials. Do you want to share?",
                "Share",
                "Decline");

            if (confirm)
            {
                // Navigate to presentation/selective disclosure flow
                var navigationParams = new Dictionary<string, object>
                {
                    ["presentationRequest"] = requestData
                };
                
                await _navigationService.NavigateToAsync("presentationrequest", navigationParams);
            }
            else
            {
                // User declined, resume scanning
                await Task.Delay(500);
                IsScanning = true;
                HasScannedData = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling presentation request");
            throw;
        }
    }

    private async Task HandleDeepLinkAsync(string deepLink)
    {
        try
        {
            _logger.LogInformation("Processing deep link: {DeepLink}", deepLink);
            
            // Handle app deep links
            // credvault://credential-offer?...
            // credvault://presentation-request?...
            // credvault://settings
            // etc.
            
            var uri = new Uri(deepLink);
            var path = uri.Host + uri.AbsolutePath;
            
            await _navigationService.NavigateToAsync(path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling deep link");
            await ShowAlertAsync("Error", "Unable to process this link.", "OK");
            
            // Resume scanning
            await Task.Delay(1000);
            IsScanning = true;
            HasScannedData = false;
        }
    }

    [RelayCommand]
    private void ToggleTorch()
    {
        IsTorchOn = !IsTorchOn;
        _logger.LogInformation("Torch toggled: {IsOn}", IsTorchOn);
    }

    [RelayCommand]
    private async Task CancelScanAsync()
    {
        await _navigationService.GoBackAsync();
    }

    [RelayCommand]
    private void ResumeScan()
    {
        IsScanning = true;
        HasScannedData = false;
        LastScannedData = null;
        _logger.LogInformation("Resumed scanning");
    }

    private async Task ShowAlertAsync(string title, string message, string button)
    {
        var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (mainPage != null)
        {
            await mainPage.DisplayAlert(title, message, button);
        }
    }

    private async Task<bool> ShowConfirmAsync(string title, string message, string accept, string cancel)
    {
        var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (mainPage != null)
        {
            return await mainPage.DisplayAlert(title, message, accept, cancel);
        }
        return false;
    }
}

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
