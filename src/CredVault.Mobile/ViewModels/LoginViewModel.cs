using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CredVault.Mobile.Models;
using CredVault.Mobile.Services;
using System.Diagnostics;

namespace CredVault.Mobile.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IIdentityApiClient _identityApiClient;
    private readonly AuthenticationFlowService? _authenticationFlowService;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool hasError;

    [ObservableProperty]
    private bool isBusy;

    public LoginViewModel(
        IIdentityApiClient identityApiClient,
        AuthenticationFlowService? authenticationFlowService = null)
    {
        _identityApiClient = identityApiClient;
        _authenticationFlowService = authenticationFlowService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;

            // Validate input
            if (string.IsNullOrWhiteSpace(Email))
            {
                HasError = true;
                ErrorMessage = "Please enter your email.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                HasError = true;
                ErrorMessage = "Please enter your password.";
                return;
            }

            // Call login API
            var request = new LoginRequestDto
            {
                Email = Email.Trim(),
                Password = Password
            };

            var response = await _identityApiClient.LoginAsync(request);

            if (response != null)
            {
                // Store tokens in SecureStorage
                await SecureStorage.SetAsync("access_token", response.AccessToken);
                await SecureStorage.SetAsync("refresh_token", response.RefreshToken);
                await SecureStorage.SetAsync("token_type", response.TokenType);
                await SecureStorage.SetAsync("expires_in", response.ExpiresIn.ToString());
                
                if (!string.IsNullOrEmpty(response.IdToken))
                {
                    await SecureStorage.SetAsync("id_token", response.IdToken);
                }

                // Store user profile if available
                if (response.User != null)
                {
                    await SecureStorage.SetAsync("user_id", response.User.UserId);
                    await SecureStorage.SetAsync("username", response.User.Username);
                    await SecureStorage.SetAsync("email", response.User.Email);
                }

                Debug.WriteLine($"✅ Login successful for user: {Email}");

                // Navigate to dashboard
                await Shell.Current.GoToAsync("//dashboard");
            }
            else
            {
                HasError = true;
                ErrorMessage = "Login failed. Please check your credentials.";
            }
        }
        catch (Refit.ApiException apiEx)
        {
            Debug.WriteLine($"❌ API Error: {apiEx.StatusCode} - {apiEx.Content}");
            HasError = true;
            
            ErrorMessage = apiEx.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => "Invalid email or password.",
                System.Net.HttpStatusCode.BadRequest => "Invalid login request. Please check your input.",
                System.Net.HttpStatusCode.TooManyRequests => "Too many login attempts. Please try again later.",
                _ => "Login failed. Please try again."
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Login error: {ex.Message}");
            HasError = true;
            ErrorMessage = "An error occurred during login. Please check your connection and try again.";
        }
        finally
        {
            IsBusy = false;
            
            // Clear password for security
            Password = string.Empty;
        }
    }

    [RelayCommand]
    private async Task NavigateToRegisterAsync()
    {
        await Shell.Current.GoToAsync("register");
    }

    [RelayCommand]
    private async Task LoginWithOAuthAsync()
    {
        // OAuth/Biometric login not yet implemented
        HasError = true;
        ErrorMessage = "Biometric login coming soon. Please use username/password.";
        await Task.CompletedTask;
    }
}
