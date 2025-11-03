using Microsoft.Extensions.Logging;

namespace CredVault.Mobile.Services;

public interface ISecurityService
{
    Task<bool> IsBiometricAvailableAsync();
    Task<bool> AuthenticateWithBiometricAsync(string reason);
    Task<bool> SetupPinAsync(string pin);
    Task<bool> VerifyPinAsync(string pin);
    Task<bool> HasPinSetAsync();
    Task ClearPinAsync();
    Task<bool> IsSecurityRequiredAsync();
    Task<bool> AuthenticateAsync(string reason);
}

public class SecurityService : ISecurityService
{
    private readonly ILogger<SecurityService> _logger;
    private const string PIN_KEY = "security_pin";
    private const string BIOMETRIC_ENABLED_KEY = "biometric_enabled";
    private const string PIN_REQUIRED_KEY = "pin_required";

    public SecurityService(ILogger<SecurityService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> IsBiometricAvailableAsync()
    {
        try
        {
            // Check if biometric authentication is available on the device
            // This is a simplified check - in production, you'd check device capabilities
            return await Task.FromResult(DeviceInfo.Platform == DevicePlatform.Android || 
                                         DeviceInfo.Platform == DevicePlatform.iOS);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking biometric availability");
            return false;
        }
    }

    public async Task<bool> AuthenticateWithBiometricAsync(string reason)
    {
        try
        {
            _logger.LogInformation("Attempting biometric authentication");

            // Check if biometrics are enabled in settings
            var biometricEnabled = Preferences.Get(BIOMETRIC_ENABLED_KEY, false);
            if (!biometricEnabled)
            {
                _logger.LogInformation("Biometric authentication is disabled");
                return false;
            }

            // Check if biometric is available
            if (!await IsBiometricAvailableAsync())
            {
                _logger.LogWarning("Biometric authentication not available on this device");
                return false;
            }

            // In a real implementation, you would use platform-specific APIs:
            // - Android: BiometricPrompt API
            // - iOS: LocalAuthentication framework (LAContext)
            // - For MAUI, you can use Plugin.Fingerprint or similar
            
            // For now, simulate biometric authentication
            await Task.Delay(500); // Simulate authentication delay
            
            var success = true; // Mock: Always succeed for demo
            _logger.LogInformation("Biometric authentication {Result}", success ? "succeeded" : "failed");
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during biometric authentication");
            return false;
        }
    }

    public async Task<bool> SetupPinAsync(string pin)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(pin) || pin.Length < 4)
            {
                _logger.LogWarning("Invalid PIN: must be at least 4 digits");
                return false;
            }

            // In production, hash the PIN before storing
            // For now, store directly (NOT SECURE - for demo only)
            await SecureStorage.SetAsync(PIN_KEY, pin);
            
            Preferences.Set(PIN_REQUIRED_KEY, true);
            _logger.LogInformation("PIN setup completed");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up PIN");
            return false;
        }
    }

    public async Task<bool> VerifyPinAsync(string pin)
    {
        try
        {
            var storedPin = await SecureStorage.GetAsync(PIN_KEY);
            
            if (string.IsNullOrEmpty(storedPin))
            {
                _logger.LogWarning("No PIN is set");
                return false;
            }

            var isValid = storedPin == pin;
            _logger.LogInformation("PIN verification {Result}", isValid ? "succeeded" : "failed");
            
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying PIN");
            return false;
        }
    }

    public async Task<bool> HasPinSetAsync()
    {
        try
        {
            var storedPin = await SecureStorage.GetAsync(PIN_KEY);
            return !string.IsNullOrEmpty(storedPin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking PIN status");
            return false;
        }
    }

    public async Task ClearPinAsync()
    {
        try
        {
            SecureStorage.Remove(PIN_KEY);
            Preferences.Set(PIN_REQUIRED_KEY, false);
            _logger.LogInformation("PIN cleared");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing PIN");
        }
    }

    public async Task<bool> IsSecurityRequiredAsync()
    {
        try
        {
            // Check if any security measure is required
            var pinRequired = Preferences.Get(PIN_REQUIRED_KEY, false);
            var biometricEnabled = Preferences.Get(BIOMETRIC_ENABLED_KEY, false);
            
            return await Task.FromResult(pinRequired || biometricEnabled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking security requirements");
            return false;
        }
    }

    public async Task<bool> AuthenticateAsync(string reason)
    {
        try
        {
            _logger.LogInformation("Starting authentication for: {Reason}", reason);

            // Try biometric first if enabled
            var biometricEnabled = Preferences.Get(BIOMETRIC_ENABLED_KEY, false);
            if (biometricEnabled && await IsBiometricAvailableAsync())
            {
                var biometricResult = await AuthenticateWithBiometricAsync(reason);
                if (biometricResult)
                {
                    return true;
                }
                // If biometric fails, fall back to PIN
            }

            // Check if PIN is required
            var pinRequired = Preferences.Get(PIN_REQUIRED_KEY, false);
            if (pinRequired && await HasPinSetAsync())
            {
                // PIN authentication will be handled by UI
                // Return false to indicate that PIN UI should be shown
                return false;
            }

            // No security configured
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication");
            return false;
        }
    }
}
