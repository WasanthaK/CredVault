using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CredVault.Mobile.Models;
using CredVault.Mobile.Services;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace CredVault.Mobile.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly IIdentityApiClient _identityApiClient;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string confirmPassword = string.Empty;

    [ObservableProperty]
    private string firstName = string.Empty;

    [ObservableProperty]
    private string lastName = string.Empty;

    [ObservableProperty]
    private string phoneNumber = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool hasError;

    [ObservableProperty]
    private string successMessage = string.Empty;

    [ObservableProperty]
    private bool hasSuccess;

    [ObservableProperty]
    private bool isBusy;

    public RegisterViewModel(IIdentityApiClient identityApiClient)
    {
        _identityApiClient = identityApiClient;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            HasError = false;
            HasSuccess = false;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            // Validate input
            if (!ValidateInput())
                return;

            // Call registration API
            var request = new RegistrationRequestDto
            {
                Username = Username.Trim(),
                Email = Email.Trim().ToLowerInvariant(),
                Password = Password,
                FirstName = string.IsNullOrWhiteSpace(FirstName) ? null : FirstName.Trim(),
                LastName = string.IsNullOrWhiteSpace(LastName) ? null : LastName.Trim(),
                PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim()
            };

            var response = await _identityApiClient.RegisterAsync(request);

            if (response?.Data != null)
            {
                HasSuccess = true;
                SuccessMessage = "Account created successfully! Redirecting to login...";
                
                Debug.WriteLine($"✅ Registration successful for user: {Username}");

                // Clear form
                ClearForm();

                // Wait a bit to show success message, then navigate to login
                await Task.Delay(2000);
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                HasError = true;
                ErrorMessage = response?.Message ?? "Registration failed. Please try again.";
            }
        }
        catch (Refit.ApiException apiEx)
        {
            Debug.WriteLine($"❌ API Error: {apiEx.StatusCode} - {apiEx.Content}");
            Debug.WriteLine($"❌ API Error Details: {apiEx.Message}");
            Debug.WriteLine($"❌ Request URI: {apiEx.RequestMessage?.RequestUri}");
            HasError = true;
            
            ErrorMessage = apiEx.StatusCode switch
            {
                System.Net.HttpStatusCode.BadRequest => ParseValidationErrors(apiEx.Content),
                System.Net.HttpStatusCode.Conflict => "Username or email already exists.",
                System.Net.HttpStatusCode.NotFound => "Registration endpoint not available. Please contact support.",
                System.Net.HttpStatusCode.Unauthorized => "API key invalid. Please contact support.",
                System.Net.HttpStatusCode.ServiceUnavailable => "Service temporarily unavailable. Please try again later.",
                _ => $"Registration failed ({apiEx.StatusCode}). Please try again."
            };
        }
        catch (HttpRequestException httpEx)
        {
            Debug.WriteLine($"❌ Network error: {httpEx.Message}");
            Debug.WriteLine($"❌ Inner exception: {httpEx.InnerException?.Message}");
            HasError = true;
            ErrorMessage = "Unable to connect to server. Please check your internet connection.";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Registration error: {ex.Message}");
            Debug.WriteLine($"❌ Error type: {ex.GetType().Name}");
            Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            HasError = true;
            ErrorMessage = $"An error occurred: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToLoginAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    private bool ValidateInput()
    {
        // Username validation
        if (string.IsNullOrWhiteSpace(Username))
        {
            HasError = true;
            ErrorMessage = "Please enter a username.";
            return false;
        }

        if (Username.Length < 3)
        {
            HasError = true;
            ErrorMessage = "Username must be at least 3 characters long.";
            return false;
        }

        // Email validation
        if (string.IsNullOrWhiteSpace(Email))
        {
            HasError = true;
            ErrorMessage = "Please enter an email address.";
            return false;
        }

        if (!IsValidEmail(Email))
        {
            HasError = true;
            ErrorMessage = "Please enter a valid email address.";
            return false;
        }

        // Password validation
        if (string.IsNullOrWhiteSpace(Password))
        {
            HasError = true;
            ErrorMessage = "Please enter a password.";
            return false;
        }

        if (Password.Length < 8)
        {
            HasError = true;
            ErrorMessage = "Password must be at least 8 characters.";
            return false;
        }

        if (!IsStrongPassword(Password))
        {
            HasError = true;
            ErrorMessage = "Password must contain:\n• 1 uppercase letter (A-Z)\n• 1 lowercase letter (a-z)\n• 1 digit (0-9)\n• 1 special character (!@#$%^&*())";
            return false;
        }

        // Confirm password validation
        if (Password != ConfirmPassword)
        {
            HasError = true;
            ErrorMessage = "Passwords do not match.";
            return false;
        }

        return true;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }

    private bool IsStrongPassword(string password)
    {
        // Must contain at least:
        // - One uppercase letter
        // - One lowercase letter
        // - One number
        // - One special character
        var hasUpperCase = new Regex(@"[A-Z]").IsMatch(password);
        var hasLowerCase = new Regex(@"[a-z]").IsMatch(password);
        var hasNumber = new Regex(@"\d").IsMatch(password);
        var hasSpecialChar = new Regex(@"[!@#$%^&*(),.?""':{}|<>]").IsMatch(password);

        return hasUpperCase && hasLowerCase && hasNumber && hasSpecialChar;
    }

    private string ParseValidationErrors(string? content)
    {
        if (string.IsNullOrEmpty(content))
            return "Invalid input. Please check your data.";

        try
        {
            // Try to parse as JSON and extract the detail field
            var jsonDoc = System.Text.Json.JsonDocument.Parse(content);
            if (jsonDoc.RootElement.TryGetProperty("detail", out var detailElement))
            {
                var detail = detailElement.GetString();
                if (!string.IsNullOrEmpty(detail))
                    return detail;
            }
            
            // Also check for errors array
            if (jsonDoc.RootElement.TryGetProperty("errors", out var errorsElement))
            {
                // Return the first error message found
                foreach (var error in errorsElement.EnumerateObject())
                {
                    if (error.Value.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        var firstError = error.Value.EnumerateArray().FirstOrDefault();
                        if (firstError.ValueKind == System.Text.Json.JsonValueKind.String)
                        {
                            return $"{error.Name}: {firstError.GetString()}";
                        }
                    }
                }
            }
        }
        catch
        {
            // If JSON parsing fails, fall back to simple string matching
        }

        // Fallback to simple string matching
        if (content.Contains("password", StringComparison.OrdinalIgnoreCase))
        {
            if (content.Contains("security requirements", StringComparison.OrdinalIgnoreCase))
                return "Password must be at least 12 characters with uppercase, lowercase, number, and special character.";
            return "Invalid password format.";
        }

        if (content.Contains("email", StringComparison.OrdinalIgnoreCase))
            return "Invalid email format.";

        if (content.Contains("username", StringComparison.OrdinalIgnoreCase))
            return "Invalid username format.";

        return "Invalid input. Please check your data.";
    }

    private void ClearForm()
    {
        Username = string.Empty;
        Email = string.Empty;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        PhoneNumber = string.Empty;
    }
}
