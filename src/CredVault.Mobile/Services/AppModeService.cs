using CredVault.Mobile.Models;
using Microsoft.Extensions.Logging;

namespace CredVault.Mobile.Services;

/// <summary>
/// Service for managing the application's operational mode (Holder, Verifier, IssuerAssist).
/// Handles mode persistence, theme switching, and mode change notifications.
/// </summary>
public class AppModeService
{
    private const string ModePreferenceKey = "app_mode";
    private readonly ILogger<AppModeService> _logger;
    private AppMode _currentMode;

    public event EventHandler<AppMode>? ModeChanged;

    public AppModeService(ILogger<AppModeService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _currentMode = LoadModeFromPreferences();
    }

    /// <summary>
    /// Gets the current application mode.
    /// </summary>
    public AppMode CurrentMode => _currentMode;

    /// <summary>
    /// Gets metadata for the current mode.
    /// </summary>
    public AppModeInfo CurrentModeInfo => AppModeInfo.GetModeInfo(_currentMode);

    /// <summary>
    /// Changes the application mode and persists the change.
    /// </summary>
    public async Task<bool> SetModeAsync(AppMode newMode)
    {
        try
        {
            if (_currentMode == newMode)
            {
                _logger.LogInformation("Mode is already set to {Mode}", newMode);
                return true;
            }

            _logger.LogInformation("Changing app mode from {OldMode} to {NewMode}", _currentMode, newMode);

            var oldMode = _currentMode;
            _currentMode = newMode;

            // Persist to preferences
            Preferences.Set(ModePreferenceKey, (int)newMode);

            // Apply theme changes
            await ApplyModeThemeAsync(newMode);

            // Notify listeners
            ModeChanged?.Invoke(this, newMode);

            _logger.LogInformation("Successfully changed mode to {Mode}", newMode);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting app mode to {Mode}", newMode);
            return false;
        }
    }

    /// <summary>
    /// Checks if the app is in a specific mode.
    /// </summary>
    public bool IsMode(AppMode mode) => _currentMode == mode;

    /// <summary>
    /// Gets all available app modes.
    /// </summary>
    public List<AppModeInfo> GetAllModes() => AppModeInfo.GetAllModes();

    /// <summary>
    /// Loads the saved mode from preferences.
    /// </summary>
    private AppMode LoadModeFromPreferences()
    {
        try
        {
            var savedMode = Preferences.Get(ModePreferenceKey, (int)AppMode.Holder);
            var mode = (AppMode)savedMode;

            if (!Enum.IsDefined(typeof(AppMode), mode))
            {
                _logger.LogWarning("Invalid mode value {Value} in preferences, defaulting to Holder", savedMode);
                return AppMode.Holder;
            }

            _logger.LogInformation("Loaded app mode from preferences: {Mode}", mode);
            return mode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading app mode from preferences, defaulting to Holder");
            return AppMode.Holder;
        }
    }

    /// <summary>
    /// Applies the theme colors for the specified mode.
    /// </summary>
    private async Task ApplyModeThemeAsync(AppMode mode)
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                var modeInfo = AppModeInfo.GetModeInfo(mode);
                var resources = Application.Current?.Resources;

                if (resources == null)
                {
                    _logger.LogWarning("Application resources not available for theme update");
                    return;
                }

                // Update primary color
                resources["Primary"] = Color.FromArgb(modeInfo.PrimaryColor);

                // Update background colors based on current theme
                var isDarkMode = Application.Current?.RequestedTheme == AppTheme.Dark;
                var backgroundColor = isDarkMode ? modeInfo.BackgroundDark : modeInfo.BackgroundLight;
                
                if (resources.ContainsKey("BackgroundColor"))
                {
                    resources["BackgroundColor"] = Color.FromArgb(backgroundColor);
                }

                _logger.LogInformation("Applied theme for mode {Mode} with primary color {Color}", mode, modeInfo.PrimaryColor);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying mode theme for {Mode}", mode);
        }
    }

    /// <summary>
    /// Gets the primary color for a specific mode.
    /// </summary>
    public Color GetPrimaryColor(AppMode mode)
    {
        var modeInfo = AppModeInfo.GetModeInfo(mode);
        return Color.FromArgb(modeInfo.PrimaryColor);
    }

    /// <summary>
    /// Gets the background color for a specific mode and theme.
    /// </summary>
    public Color GetBackgroundColor(AppMode mode, bool isDarkMode)
    {
        var modeInfo = AppModeInfo.GetModeInfo(mode);
        var colorHex = isDarkMode ? modeInfo.BackgroundDark : modeInfo.BackgroundLight;
        return Color.FromArgb(colorHex);
    }

    /// <summary>
    /// Initializes the service and applies the saved mode's theme.
    /// Should be called during app startup.
    /// </summary>
    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing AppModeService with mode: {Mode}", _currentMode);
        await ApplyModeThemeAsync(_currentMode);
    }
}
