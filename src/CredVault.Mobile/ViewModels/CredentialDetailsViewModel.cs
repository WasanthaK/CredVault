using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CredVault.Mobile.Models;
using CredVault.Mobile.Services;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace CredVault.Mobile.ViewModels;

public partial class CredentialDetailsViewModel : BaseViewModel
{
    private readonly WalletService _walletService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<CredentialDetailsViewModel> _logger;

    [ObservableProperty]
    private CredentialResponseDto? credential;

    [ObservableProperty]
    private ObservableCollection<ClaimItem> claims = new();

    [ObservableProperty]
    private string statusColor = "#9E9E9E";

    [ObservableProperty]
    private bool canRevoke;

    [ObservableProperty]
    private bool canSuspend;

    [ObservableProperty]
    private bool isExpired;

    [ObservableProperty]
    private string qrCodeData = string.Empty;

    public CredentialDetailsViewModel(
        WalletService walletService,
        INavigationService navigationService,
        ILogger<CredentialDetailsViewModel> logger)
    {
        _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Title = "Credential Details";
    }

    public async Task InitializeAsync(string credentialId)
    {
        if (string.IsNullOrWhiteSpace(credentialId))
        {
            ErrorMessage = "Invalid credential ID";
            HasError = true;
            return;
        }

        await LoadCredentialAsync(credentialId);
    }

    [RelayCommand]
    private async Task LoadCredentialAsync(string credentialId)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;

            _logger.LogInformation("Loading credential details: {CredentialId}", credentialId);

            if (!Guid.TryParse(credentialId, out var guid))
            {
                HasError = true;
                ErrorMessage = "Invalid credential ID format";
                return;
            }

            var result = await _walletService.GetCredentialByIdAsync(guid);

            if (result.IsSuccess && result.Data != null)
            {
                Credential = result.Data;
                UpdateCredentialState();
                ParseClaims();
                GenerateQRCode();

                _logger.LogInformation("Loaded credential: {Type}", Credential.Type);
            }
            else
            {
                HasError = true;
                ErrorMessage = result.ErrorMessage ?? "Failed to load credential";
                _logger.LogWarning("Failed to load credential: {Error}", ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = "An unexpected error occurred";
            _logger.LogError(ex, "Error loading credential details");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RevokeCredentialAsync()
    {
        if (Credential == null || !CanRevoke)
            return;

        try
        {
            var confirmed = await ShowConfirmAsync(
                "Revoke Credential",
                "Are you sure you want to revoke this credential? This action cannot be undone.",
                "Revoke",
                "Cancel");

            if (!confirmed)
                return;

            IsBusy = true;
            _logger.LogInformation("Revoking credential: {CredentialId}", Credential.CredentialId);

            if (!Guid.TryParse(Credential.CredentialId, out var guid))
            {
                ErrorMessage = "Invalid credential ID format";
                return;
            }

            var result = await _walletService.RevokeCredentialAsync(guid);

            if (result.IsSuccess)
            {
                await ShowAlertAsync("Success", "Credential revoked successfully");

                // Reload credential to show updated status
                if (Credential.CredentialId != null)
                {
                    await LoadCredentialAsync(Credential.CredentialId);
                }
            }
            else
            {
                await ShowAlertAsync("Error", result.ErrorMessage ?? "Failed to revoke credential");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking credential");
            await ShowAlertAsync("Error", "An unexpected error occurred");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SuspendCredentialAsync()
    {
        if (Credential == null || !CanSuspend)
            return;

        try
        {
            var confirmed = await ShowConfirmAsync(
                "Suspend Credential",
                "Do you want to temporarily suspend this credential?",
                "Suspend",
                "Cancel");

            if (!confirmed)
                return;

            IsBusy = true;
            _logger.LogInformation("Suspending credential: {CredentialId}", Credential.CredentialId);

            if (!Guid.TryParse(Credential.CredentialId, out var guid))
            {
                ErrorMessage = "Invalid credential ID format";
                return;
            }

            var result = await _walletService.SuspendCredentialAsync(guid);

            if (result.IsSuccess)
            {
                await ShowAlertAsync("Success", "Credential suspended successfully");

                // Reload credential to show updated status
                if (Credential.CredentialId != null)
                {
                    await LoadCredentialAsync(Credential.CredentialId);
                }
            }
            else
            {
                await ShowAlertAsync("Error", result.ErrorMessage ?? "Failed to suspend credential");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending credential");
            await ShowAlertAsync("Error", "An unexpected error occurred");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ShareCredentialAsync()
    {
        if (Credential == null)
            return;

        try
        {
            _logger.LogInformation("Sharing credential: {CredentialId}", Credential.CredentialId);

            // TODO: Implement proper credential sharing via OpenID4VP
            await ShowAlertAsync(
                "Share Credential",
                "Credential sharing will be implemented in the presentation flow");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sharing credential");
            await ShowAlertAsync("Error", "Failed to share credential");
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        try
        {
            await _navigationService.GoBackAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating back");
        }
    }

    private void UpdateCredentialState()
    {
        if (Credential == null)
            return;

        // Update status color
        StatusColor = Credential.Status switch
        {
            CredentialStatus.Active => "#4CAF50",      // Green
            CredentialStatus.Expired => "#F44336",     // Red
            CredentialStatus.Revoked => "#9E9E9E",     // Gray
            CredentialStatus.Suspended => "#FF9800",   // Orange
            _ => "#9E9E9E"
        };

        // Update action availability
        CanRevoke = Credential.Status == CredentialStatus.Active || 
                    Credential.Status == CredentialStatus.Suspended;
        CanSuspend = Credential.Status == CredentialStatus.Active;

        // Check expiration
        IsExpired = Credential.ExpiresAt.HasValue && 
                    Credential.ExpiresAt.Value < DateTime.UtcNow;
    }

    private void ParseClaims()
    {
        Claims.Clear();

        if (Credential?.Claims == null)
            return;

        foreach (var claim in Credential.Claims)
        {
            Claims.Add(new ClaimItem
            {
                Key = claim.Key,
                Value = claim.Value?.ToString() ?? string.Empty
            });
        }

        _logger.LogDebug("Parsed {Count} claims", Claims.Count);
    }

    private void GenerateQRCode()
    {
        if (Credential?.CredentialId == null)
            return;

        // Generate QR code data for credential presentation
        // Format: credvault://present?credential_id={id}
        QrCodeData = $"credvault://present?credential_id={Credential.CredentialId}";
        
        _logger.LogDebug("Generated QR code data for credential");
    }

    private async Task ShowAlertAsync(string title, string message, string button = "OK")
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page != null)
        {
            await page.DisplayAlert(title, message, button);
        }
    }

    private async Task<bool> ShowConfirmAsync(string title, string message, string accept, string cancel)
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page != null)
        {
            return await page.DisplayAlert(title, message, accept, cancel);
        }
        return false;
    }
}

/// <summary>
/// Helper class for displaying claims in the UI
/// </summary>
public class ClaimItem
{
    public required string Key { get; set; }
    public required string Value { get; set; }
}
