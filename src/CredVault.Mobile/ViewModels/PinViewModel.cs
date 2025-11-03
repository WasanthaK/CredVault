using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CredVault.Mobile.Services;
using Microsoft.Extensions.Logging;

namespace CredVault.Mobile.ViewModels;

public partial class PinViewModel : ObservableObject
{
    private readonly ISecurityService _securityService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<PinViewModel> _logger;

    [ObservableProperty]
    private string title = "Enter PIN";

    [ObservableProperty]
    private string instruction = "Enter your 4-digit PIN";

    [ObservableProperty]
    private string pin = string.Empty;

    [ObservableProperty]
    private string confirmPin = string.Empty;

    [ObservableProperty]
    private bool isSetupMode;

    [ObservableProperty]
    private bool isConfirmStep;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool hasError;

    [ObservableProperty]
    private int pinLength;

    [ObservableProperty]
    private int confirmPinLength;

    public Action<bool>? OnAuthenticationComplete { get; set; }

    public PinViewModel(
        ISecurityService securityService,
        INavigationService navigationService,
        ILogger<PinViewModel> logger)
    {
        _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void SetMode(bool isSetup, string? customInstruction = null)
    {
        IsSetupMode = isSetup;
        Title = isSetup ? "Setup PIN" : "Enter PIN";
        Instruction = customInstruction ?? (isSetup ? "Create a 4-digit PIN" : "Enter your PIN to continue");
        IsConfirmStep = false;
        ClearPin();
    }

    [RelayCommand]
    private void AddDigit(string digit)
    {
        if (IsSetupMode && IsConfirmStep)
        {
            if (ConfirmPin.Length < 4)
            {
                ConfirmPin += digit;
                ConfirmPinLength = ConfirmPin.Length;
                
                if (ConfirmPin.Length == 4)
                {
                    MainThread.BeginInvokeOnMainThread(async () => await ValidateAndSavePin());
                }
            }
        }
        else
        {
            if (Pin.Length < 4)
            {
                Pin += digit;
                PinLength = Pin.Length;
                HasError = false;
                ErrorMessage = string.Empty;
                
                if (Pin.Length == 4)
                {
                    if (IsSetupMode)
                    {
                        MoveToConfirmStep();
                    }
                    else
                    {
                        MainThread.BeginInvokeOnMainThread(async () => await VerifyPinAsync());
                    }
                }
            }
        }
    }

    [RelayCommand]
    private void DeleteDigit()
    {
        if (IsSetupMode && IsConfirmStep)
        {
            if (ConfirmPin.Length > 0)
            {
                ConfirmPin = ConfirmPin.Substring(0, ConfirmPin.Length - 1);
                ConfirmPinLength = ConfirmPin.Length;
            }
        }
        else
        {
            if (Pin.Length > 0)
            {
                Pin = Pin.Substring(0, Pin.Length - 1);
                PinLength = Pin.Length;
                HasError = false;
                ErrorMessage = string.Empty;
            }
        }
    }

    [RelayCommand]
    private void ClearPin()
    {
        Pin = string.Empty;
        ConfirmPin = string.Empty;
        PinLength = 0;
        ConfirmPinLength = 0;
        HasError = false;
        ErrorMessage = string.Empty;
    }

    private void MoveToConfirmStep()
    {
        IsConfirmStep = true;
        Title = "Confirm PIN";
        Instruction = "Re-enter your PIN to confirm";
    }

    private async Task ValidateAndSavePin()
    {
        try
        {
            if (Pin != ConfirmPin)
            {
                HasError = true;
                ErrorMessage = "PINs do not match. Please try again.";
                _logger.LogWarning("PIN confirmation failed: mismatch");
                
                await Task.Delay(1500);
                
                // Reset to first step
                IsConfirmStep = false;
                Title = "Setup PIN";
                Instruction = "Create a 4-digit PIN";
                ClearPin();
                return;
            }

            var success = await _securityService.SetupPinAsync(Pin);
            
            if (success)
            {
                _logger.LogInformation("PIN setup successful");
                OnAuthenticationComplete?.Invoke(true);
                await _navigationService.GoBackAsync();
            }
            else
            {
                HasError = true;
                ErrorMessage = "Failed to save PIN. Please try again.";
                _logger.LogError("Failed to save PIN");
                ClearPin();
                IsConfirmStep = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during PIN setup");
            HasError = true;
            ErrorMessage = "An error occurred. Please try again.";
            ClearPin();
            IsConfirmStep = false;
        }
    }

    private async Task VerifyPinAsync()
    {
        try
        {
            var isValid = await _securityService.VerifyPinAsync(Pin);
            
            if (isValid)
            {
                _logger.LogInformation("PIN verification successful");
                OnAuthenticationComplete?.Invoke(true);
                await _navigationService.GoBackAsync();
            }
            else
            {
                HasError = true;
                ErrorMessage = "Incorrect PIN. Please try again.";
                _logger.LogWarning("PIN verification failed");
                
                await Task.Delay(1500);
                ClearPin();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during PIN verification");
            HasError = true;
            ErrorMessage = "An error occurred. Please try again.";
            ClearPin();
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        OnAuthenticationComplete?.Invoke(false);
        await _navigationService.GoBackAsync();
    }

    [RelayCommand]
    private async Task UseBiometricAsync()
    {
        try
        {
            var success = await _securityService.AuthenticateWithBiometricAsync("Authenticate to continue");
            
            if (success)
            {
                _logger.LogInformation("Biometric authentication successful");
                OnAuthenticationComplete?.Invoke(true);
                await _navigationService.GoBackAsync();
            }
            else
            {
                HasError = true;
                ErrorMessage = "Biometric authentication failed";
                _logger.LogWarning("Biometric authentication failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during biometric authentication");
            HasError = true;
            ErrorMessage = "Biometric authentication error";
        }
    }
}
