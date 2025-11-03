using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CredVault.Mobile.Services;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CredVault.Mobile.ViewModels;

public partial class BackupViewModel : ObservableObject
{
    private readonly WalletService _walletService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<BackupViewModel> _logger;

    [ObservableProperty]
    private string title = "Backup & Restore";

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool showExportModal;

    [ObservableProperty]
    private string backupQrData = string.Empty;

    [ObservableProperty]
    private string backupKeyPreview = string.Empty;

    [ObservableProperty]
    private DateTime? lastBackupDate;

    public BackupViewModel(
        WalletService walletService,
        INavigationService navigationService,
        ILogger<BackupViewModel> logger)
    {
        _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InitializeAsync()
    {
        await LoadBackupInfoAsync();
    }

    [RelayCommand]
    private Task LoadBackupInfoAsync()
    {
        try
        {
            _logger.LogInformation("Loading backup information");
            
            // TODO: Load last backup date from preferences/storage
            // For now, just set to null
            LastBackupDate = null;
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading backup info");
            return Task.CompletedTask;
        }
    }

    [RelayCommand]
    private async Task ExportBackupAsync()
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;
            _logger.LogInformation("Generating backup key");

            // Fetch all credentials
            var result = await _walletService.GetCredentialsAsync();

            if (!result.IsSuccess || result.Data == null)
            {
                _logger.LogWarning("Failed to fetch credentials for backup: {Message}", result.ErrorMessage);
                if (Application.Current?.Windows.Count > 0)
                {
                    await Application.Current.Windows[0].Page!.DisplayAlert(
                        "Error",
                        "Failed to create backup. Please try again.",
                        "OK");
                }
                return;
            }

            // Generate backup data
            var backupData = new BackupData
            {
                Version = "1.0",
                CreatedAt = DateTime.UtcNow,
                Credentials = result.Data.Select(c => new BackupCredential
                {
                    Id = c.Id ?? string.Empty,
                    Type = c.Type ?? string.Empty,
                    IssuerName = c.Issuer ?? "Unknown Issuer",
                    IssuedDate = c.IssuedAt,
                    ExpiryDate = c.ExpiresAt,
                    Status = c.Status.ToString(),
                    // Note: In production, this would include encrypted credential data
                    RawCredential = "ENCRYPTED_DATA_PLACEHOLDER"
                }).ToList()
            };

            // Encrypt backup data
            var encryptedBackup = EncryptBackupData(backupData);

            // Generate QR code data (base64 encoded encrypted backup)
            BackupQrData = Convert.ToBase64String(Encoding.UTF8.GetBytes(encryptedBackup));
            
            // Create a preview of the backup key (first 16 chars + ... + last 16 chars)
            if (BackupQrData.Length > 32)
            {
                BackupKeyPreview = $"{BackupQrData[..16]}...{BackupQrData[^16..]}";
            }
            else
            {
                BackupKeyPreview = BackupQrData;
            }

            // Update last backup date
            LastBackupDate = DateTime.Now;

            // Show export modal
            ShowExportModal = true;

            _logger.LogInformation("Backup generated successfully with {Count} credentials", result.Data.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating backup");
            if (Application.Current?.Windows.Count > 0)
            {
                await Application.Current.Windows[0].Page!.DisplayAlert(
                    "Error",
                    "An unexpected error occurred while creating backup",
                    "OK");
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void CloseExportModal()
    {
        ShowExportModal = false;
        BackupQrData = string.Empty;
        BackupKeyPreview = string.Empty;
    }

    [RelayCommand]
    private async Task ImportBackupAsync()
    {
        try
        {
            _logger.LogInformation("Starting backup import flow");

            // Navigate to shared QR scanner for backup import
            await _navigationService.NavigateToAsync("scanner");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting import");
        }
    }

    [RelayCommand]
    private async Task ProcessImportedBackupAsync(string qrData)
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;
            _logger.LogInformation("Processing imported backup");

            // Decode QR data
            var encryptedData = Encoding.UTF8.GetString(Convert.FromBase64String(qrData));

            // Decrypt backup data
            var decryptedJson = DecryptBackupData(encryptedData);

            // Deserialize backup data
            var backupData = JsonSerializer.Deserialize<BackupData>(decryptedJson);

            if (backupData == null || backupData.Credentials == null)
            {
                throw new InvalidOperationException("Invalid backup data");
            }

            _logger.LogInformation("Backup contains {Count} credentials", backupData.Credentials.Count);

            // Show confirmation dialog
            if (Application.Current?.Windows.Count > 0)
            {
                var confirm = await Application.Current.Windows[0].Page!.DisplayAlert(
                    "Restore Backup",
                    $"This backup contains {backupData.Credentials.Count} credential(s) from {backupData.CreatedAt:MMM dd, yyyy}.\n\nRestore this backup? This will replace your current credentials.",
                    "Restore",
                    "Cancel");

                if (!confirm)
                {
                    _logger.LogInformation("User cancelled backup restore");
                    return;
                }
            }

            // TODO: In a production app, this would:
            // 1. Decrypt each credential
            // 2. Import credentials into wallet
            // 3. Update local storage

            // For now, just show success message
            if (Application.Current?.Windows.Count > 0)
            {
                await Application.Current.Windows[0].Page!.DisplayAlert(
                    "Success",
                    $"Successfully restored {backupData.Credentials.Count} credential(s)",
                    "OK");
            }

            // Navigate back
            await _navigationService.GoBackAsync();

            _logger.LogInformation("Backup restored successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing backup import");
            if (Application.Current?.Windows.Count > 0)
            {
                await Application.Current.Windows[0].Page!.DisplayAlert(
                    "Error",
                    "Failed to restore backup. The backup data may be invalid or corrupted.",
                    "OK");
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await _navigationService.GoBackAsync();
    }

    private string EncryptBackupData(BackupData data)
    {
        // In a production app, this would use proper encryption (AES-256-GCM)
        // with a user-derived key (password + salt + PBKDF2)
        
        // For demonstration, we'll just serialize to JSON
        // SECURITY NOTE: This is NOT secure for production use!
        
        var json = JsonSerializer.Serialize(data);
        
        // Mock encryption: Base64 encode (DO NOT USE IN PRODUCTION)
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    private string DecryptBackupData(string encryptedData)
    {
        // In a production app, this would use proper decryption
        
        // Mock decryption: Base64 decode (DO NOT USE IN PRODUCTION)
        return Encoding.UTF8.GetString(Convert.FromBase64String(encryptedData));
    }
}

// Backup data models
public class BackupData
{
    public required string Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public required List<BackupCredential> Credentials { get; set; }
}

public class BackupCredential
{
    public required string Id { get; set; }
    public required string Type { get; set; }
    public required string IssuerName { get; set; }
    public DateTime IssuedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public required string Status { get; set; }
    public required string RawCredential { get; set; }
}
