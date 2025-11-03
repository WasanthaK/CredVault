using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CredVault.Mobile.Models;
using CredVault.Mobile.Services;
using System.Diagnostics;

namespace CredVault.Mobile.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IIdentityApiClient _identityApiClient;
    private readonly IAuthenticationFlowService _authenticationFlowService;

    [ObservableProperty]
    private string username = string.Empty;

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
        IAuthenticationFlowService authenticationFlowService)
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
            if (string.IsNullOrWhiteSpace(Username))
            {
                HasError = true;
                ErrorMessage = "Please enter your username.";
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
                Username = Username.Trim(),
                Password = Password
            };

            var response = await _identityApiClient.LoginAsync(request);

            if (response?.Data != null)
            {
                // Store tokens in SecureStorage
                await SecureStorage.SetAsync("access_token", response.Data.AccessToken);
                await SecureStorage.SetAsync("refresh_token", response.Data.RefreshToken);
                await SecureStorage.SetAsync("token_type", response.Data.TokenType);
                await SecureStorage.SetAsync("expires_in", response.Data.ExpiresIn.ToString());
                
                if (!string.IsNullOrEmpty(response.Data.IdToken))
                {
                    await SecureStorage.SetAsync("id_token", response.Data.IdToken);
                }

                // Store user profile if available
                if (response.Data.UserProfile != null)
                {
                    await SecureStorage.SetAsync("user_id", response.Data.UserProfile.UserId);
                    await SecureStorage.SetAsync("username", response.Data.UserProfile.Username);
                    await SecureStorage.SetAsync("email", response.Data.UserProfile.Email);
                }

                Debug.WriteLine($"✅ Login successful for user: {Username}");

                // Navigate to dashboard
                await Shell.Current.GoToAsync("//dashboard");
            }
            else
            {
                HasError = true;
                ErrorMessage = response?.Message ?? "Login failed. Please check your credentials.";
            }
        }
        catch (Refit.ApiException apiEx)
        {
            Debug.WriteLine($"❌ API Error: {apiEx.StatusCode} - {apiEx.Content}");
            HasError = true;
            
            ErrorMessage = apiEx.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => "Invalid username or password.",
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
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;

            // Use the authentication flow service for OAuth2/OIDC flow
            var success = await _authenticationFlowService.AuthorizeAsync();

            if (success)
            {
                Debug.WriteLine("✅ OAuth login successful");
                await Shell.Current.GoToAsync("//dashboard");
            }
            else
            {
                HasError = true;
                ErrorMessage = "OAuth login was cancelled or failed.";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ OAuth login error: {ex.Message}");
            HasError = true;
            ErrorMessage = "OAuth login failed. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
