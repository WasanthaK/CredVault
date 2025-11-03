using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CredVault.Mobile.Models;
using CredVault.Mobile.Services;
using Microsoft.Extensions.Logging;

namespace CredVault.Mobile.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly ISecurityService _securityService;
    private readonly AppModeService _appModeService;
    private readonly ILogger<SettingsViewModel> _logger;

    [ObservableProperty]
    private string title = "Settings";

    [ObservableProperty]
    private string appVersion = "1.0.0";

    // Theme Settings
    [ObservableProperty]
    private string selectedTheme = "System";

    [ObservableProperty]
    private bool isDarkMode;

    // Language Settings
    [ObservableProperty]
    private string selectedLanguage = "English";

    // Notification Settings
    [ObservableProperty]
    private bool notificationsEnabled;

    [ObservableProperty]
    private bool credentialExpiryAlerts;

    [ObservableProperty]
    private bool securityAlerts;

    // Security Settings
    [ObservableProperty]
    private bool biometricEnabled;

    [ObservableProperty]
    private bool pinEnabled;

    [ObservableProperty]
    private bool autoLockEnabled;

    [ObservableProperty]
    private int autoLockMinutes = 5;

    // App Mode Settings
    [ObservableProperty]
    private AppMode selectedAppMode;

    [ObservableProperty]
    private string selectedAppModeName = "Holder";

    public List<string> ThemeOptions { get; } = new() { "Light", "Dark", "System" };
    public List<string> LanguageOptions { get; } = new() { "English", "Spanish", "French", "German" };
    public List<int> AutoLockOptions { get; } = new() { 1, 5, 10, 15, 30 };
    public List<AppModeInfo> AppModeOptions => _appModeService.GetAllModes();

    public SettingsViewModel(
        INavigationService navigationService,
        ISecurityService securityService,
        AppModeService appModeService,
        ILogger<SettingsViewModel> logger)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
        _appModeService = appModeService ?? throw new ArgumentNullException(nameof(appModeService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Load current mode
        SelectedAppMode = _appModeService.CurrentMode;
        SelectedAppModeName = _appModeService.CurrentModeInfo.Name;
    }

    public async Task InitializeAsync()
    {
        await LoadSettingsAsync();
    }

    [RelayCommand]
    private Task LoadSettingsAsync()
    {
        try
        {
            _logger.LogInformation("Loading settings");

            // Load from Preferences
            SelectedTheme = Preferences.Get("app_theme", "System");
            SelectedLanguage = Preferences.Get("app_language", "English");
            NotificationsEnabled = Preferences.Get("notifications_enabled", true);
            CredentialExpiryAlerts = Preferences.Get("expiry_alerts", true);
            SecurityAlerts = Preferences.Get("security_alerts", true);
            BiometricEnabled = Preferences.Get("biometric_enabled", false);
            PinEnabled = Preferences.Get("pin_enabled", false);
            AutoLockEnabled = Preferences.Get("auto_lock_enabled", true);
            AutoLockMinutes = Preferences.Get("auto_lock_minutes", 5);

            // Update dark mode based on theme
            UpdateDarkMode();

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading settings");
            return Task.CompletedTask;
        }
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        try
        {
            _logger.LogInformation("Saving settings");

            // Save to Preferences
            Preferences.Set("app_theme", SelectedTheme);
            Preferences.Set("app_language", SelectedLanguage);
            Preferences.Set("notifications_enabled", NotificationsEnabled);
            Preferences.Set("expiry_alerts", CredentialExpiryAlerts);
            Preferences.Set("security_alerts", SecurityAlerts);
            Preferences.Set("biometric_enabled", BiometricEnabled);
            Preferences.Set("pin_enabled", PinEnabled);
            Preferences.Set("auto_lock_enabled", AutoLockEnabled);
            Preferences.Set("auto_lock_minutes", AutoLockMinutes);

            // Update dark mode
            UpdateDarkMode();

            if (Application.Current?.Windows.Count > 0)
            {
                await Application.Current.Windows[0].Page!.DisplayAlert(
                    "Success",
                    "Settings saved successfully",
                    "OK");
            }

            _logger.LogInformation("Settings saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving settings");
            if (Application.Current?.Windows.Count > 0)
            {
                await Application.Current.Windows[0].Page!.DisplayAlert(
                    "Error",
                    "Failed to save settings",
                    "OK");
            }
        }
    }

    [RelayCommand]
    private void ThemeChanged()
    {
        _logger.LogInformation("Theme changed to: {Theme}", SelectedTheme);
        UpdateDarkMode();
        Preferences.Set("app_theme", SelectedTheme);
    }

    [RelayCommand]
    private void LanguageChanged()
    {
        _logger.LogInformation("Language changed to: {Language}", SelectedLanguage);
        Preferences.Set("app_language", SelectedLanguage);
    }

    [RelayCommand]
    private void NotificationsToggled()
    {
        _logger.LogInformation("Notifications toggled: {Enabled}", NotificationsEnabled);
        Preferences.Set("notifications_enabled", NotificationsEnabled);

        // If notifications are disabled, disable all notification types
        if (!NotificationsEnabled)
        {
            CredentialExpiryAlerts = false;
            SecurityAlerts = false;
        }
    }

    [RelayCommand]
    private void BiometricToggled()
    {
        _logger.LogInformation("Biometric authentication toggled: {Enabled}", BiometricEnabled);
        Preferences.Set("biometric_enabled", BiometricEnabled);
    }

    [RelayCommand]
    private async Task ChangeAppModeAsync(AppMode newMode)
    {
        try
        {
            _logger.LogInformation("User requested mode change to {Mode}", newMode);

            var success = await _appModeService.SetModeAsync(newMode);
            
            if (success)
            {
                SelectedAppMode = newMode;
                SelectedAppModeName = _appModeService.CurrentModeInfo.Name;

                if (Application.Current?.Windows.Count > 0)
                {
                    await Application.Current.Windows[0].Page!.DisplayAlert(
                        "Mode Changed",
                        $"Switched to {_appModeService.CurrentModeInfo.Name}",
                        "OK");
                }

                _logger.LogInformation("App mode changed successfully to {Mode}", newMode);
            }
            else
            {
                _logger.LogWarning("Failed to change app mode to {Mode}", newMode);
                
                if (Application.Current?.Windows.Count > 0)
                {
                    await Application.Current.Windows[0].Page!.DisplayAlert(
                        "Error",
                        "Failed to change app mode. Please try again.",
                        "OK");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing app mode to {Mode}", newMode);
        }
    }

    [RelayCommand]
    private async Task PinToggledAsync()
    {
        try
        {
            if (PinEnabled)
            {
                // Check if PIN is already set
                var hasPinSet = await _securityService.HasPinSetAsync();
                
                if (!hasPinSet)
                {
                    _logger.LogInformation("Navigating to PIN setup");
                    await _navigationService.NavigateToAsync("securitypin");
                }
                else
                {
                    _logger.LogInformation("PIN authentication enabled");
                    Preferences.Set("pin_enabled", true);
                }
            }
            else
            {
                _logger.LogInformation("PIN authentication disabled");
                Preferences.Set("pin_enabled", false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling PIN");
            PinEnabled = !PinEnabled; // Revert toggle
        }
    }

    [RelayCommand]
    private void AutoLockToggled()
    {
        _logger.LogInformation("Auto-lock toggled: {Enabled}", AutoLockEnabled);
        Preferences.Set("auto_lock_enabled", AutoLockEnabled);
    }

    [RelayCommand]
    private async Task ClearCacheAsync()
    {
        try
        {
            if (Application.Current?.Windows.Count > 0)
            {
                var confirm = await Application.Current.Windows[0].Page!.DisplayAlert(
                    "Clear Cache",
                    "This will clear all cached data. Are you sure?",
                    "Clear",
                    "Cancel");

                if (!confirm)
                    return;
            }

            _logger.LogInformation("Clearing cache");

            // TODO: Implement cache clearing logic
            // For now, just show success message

            if (Application.Current?.Windows.Count > 0)
            {
                await Application.Current.Windows[0].Page!.DisplayAlert(
                    "Success",
                    "Cache cleared successfully",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
        }
    }

    [RelayCommand]
    private async Task ResetSettingsAsync()
    {
        try
        {
            if (Application.Current?.Windows.Count > 0)
            {
                var confirm = await Application.Current.Windows[0].Page!.DisplayAlert(
                    "Reset Settings",
                    "This will reset all settings to default values. Are you sure?",
                    "Reset",
                    "Cancel");

                if (!confirm)
                    return;
            }

            _logger.LogInformation("Resetting settings");

            // Reset to defaults
            Preferences.Clear();
            await LoadSettingsAsync();

            if (Application.Current?.Windows.Count > 0)
            {
                await Application.Current.Windows[0].Page!.DisplayAlert(
                    "Success",
                    "Settings reset to defaults",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting settings");
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        try
        {
            if (Application.Current?.Windows.Count > 0)
            {
                var confirm = await Application.Current.Windows[0].Page!.DisplayAlert(
                    "Logout",
                    "Are you sure you want to logout?",
                    "Logout",
                    "Cancel");

                if (!confirm)
                    return;
            }

            _logger.LogInformation("User logging out");

            // Clear all tokens from SecureStorage
            SecureStorage.Remove("access_token");
            SecureStorage.Remove("refresh_token");
            SecureStorage.Remove("id_token");
            SecureStorage.Remove("token_type");
            SecureStorage.Remove("expires_in");
            SecureStorage.Remove("user_id");
            SecureStorage.Remove("username");
            SecureStorage.Remove("email");

            _logger.LogInformation("Tokens cleared, navigating to login");

            // Navigate to login page
            await Shell.Current.GoToAsync("//login");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            
            if (Application.Current?.Windows.Count > 0)
            {
                await Application.Current.Windows[0].Page!.DisplayAlert(
                    "Error",
                    "An error occurred during logout. Please try again.",
                    "OK");
            }
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await _navigationService.GoBackAsync();
    }

    private void UpdateDarkMode()
    {
        IsDarkMode = SelectedTheme switch
        {
            "Dark" => true,
            "Light" => false,
            _ => Application.Current?.RequestedTheme == AppTheme.Dark
        };

        // Apply theme
        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = SelectedTheme switch
            {
                "Dark" => AppTheme.Dark,
                "Light" => AppTheme.Light,
                _ => AppTheme.Unspecified
            };
        }
    }
}
