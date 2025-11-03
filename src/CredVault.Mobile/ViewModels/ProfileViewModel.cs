using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CredVault.Mobile.Models;
using CredVault.Mobile.Services;
using Microsoft.Extensions.Logging;

namespace CredVault.Mobile.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly IdentityService _identityService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<ProfileViewModel> _logger;

    [ObservableProperty]
    private string title = "My Profile";

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isEditing;

    [ObservableProperty]
    private string userId = string.Empty;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private bool isEmailVerified;

    [ObservableProperty]
    private string phoneNumber = string.Empty;

    [ObservableProperty]
    private bool isPhoneVerified;

    [ObservableProperty]
    private string firstName = string.Empty;

    [ObservableProperty]
    private string lastName = string.Empty;

    [ObservableProperty]
    private string profilePictureUrl = string.Empty;

    [ObservableProperty]
    private DateTime? dateOfBirth;

    [ObservableProperty]
    private string gender = string.Empty;

    [ObservableProperty]
    private string country = string.Empty;

    [ObservableProperty]
    private string language = string.Empty;

    [ObservableProperty]
    private string timeZone = string.Empty;

    [ObservableProperty]
    private DateTime? lastLoginAt;

    [ObservableProperty]
    private string displayName = string.Empty;

    [ObservableProperty]
    private string initials = string.Empty;

    public ProfileViewModel(
        IdentityService identityService,
        INavigationService navigationService,
        ILogger<ProfileViewModel> logger)
    {
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InitializeAsync()
    {
        await LoadProfileAsync();
    }

    [RelayCommand]
    private async Task LoadProfileAsync()
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;
            _logger.LogInformation("Loading user profile");

            var result = await _identityService.GetProfileAsync();

            if (result.IsSuccess && result.Data != null)
            {
                PopulateFromDto(result.Data);
                _logger.LogInformation("Successfully loaded profile for user: {UserId}", UserId);
            }
            else
            {
                _logger.LogWarning("Failed to load profile: {Message}", result.ErrorMessage);
                if (Application.Current?.Windows.Count > 0)
                {
                    await Application.Current.Windows[0].Page!.DisplayAlert(
                        "Error",
                        result.ErrorMessage ?? "Failed to load profile",
                        "OK");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading profile");
            if (Application.Current?.Windows.Count > 0)
            {
                await Application.Current.Windows[0].Page!.DisplayAlert(
                    "Error",
                    "An unexpected error occurred while loading your profile",
                    "OK");
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ToggleEdit()
    {
        IsEditing = !IsEditing;
        
        if (!IsEditing)
        {
            // If canceling edit, reload the profile to reset changes
            _ = LoadProfileAsync();
        }
    }

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;
            _logger.LogInformation("Saving profile changes");

            var request = new UpdateProfileRequestDto
            {
                Email = Email,
                PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber,
                FirstName = string.IsNullOrWhiteSpace(FirstName) ? null : FirstName,
                LastName = string.IsNullOrWhiteSpace(LastName) ? null : LastName,
                ProfilePictureUrl = string.IsNullOrWhiteSpace(ProfilePictureUrl) ? null : ProfilePictureUrl,
                DateOfBirth = DateOfBirth,
                Gender = string.IsNullOrWhiteSpace(Gender) ? null : Gender,
                Country = string.IsNullOrWhiteSpace(Country) ? null : Country,
                Language = string.IsNullOrWhiteSpace(Language) ? null : Language,
                TimeZone = string.IsNullOrWhiteSpace(TimeZone) ? null : TimeZone
            };

            var result = await _identityService.UpdateProfileAsync(request);

            if (result.IsSuccess && result.Data != null)
            {
                PopulateFromDto(result.Data);
                IsEditing = false;
                _logger.LogInformation("Successfully saved profile changes");
                
                if (Application.Current?.Windows.Count > 0)
                {
                    await Application.Current.Windows[0].Page!.DisplayAlert(
                        "Success",
                        "Your profile has been updated successfully",
                        "OK");
                }
            }
            else
            {
                _logger.LogWarning("Failed to save profile: {Message}", result.ErrorMessage);
                if (Application.Current?.Windows.Count > 0)
                {
                    await Application.Current.Windows[0].Page!.DisplayAlert(
                        "Error",
                        result.ErrorMessage ?? "Failed to save profile changes",
                        "OK");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving profile");
            if (Application.Current?.Windows.Count > 0)
            {
                await Application.Current.Windows[0].Page!.DisplayAlert(
                    "Error",
                    "An unexpected error occurred while saving your profile",
                    "OK");
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ChangeProfilePictureAsync()
    {
        try
        {
            // TODO: Implement image picker functionality
            // For now, show a placeholder message
            if (Application.Current?.Windows.Count > 0)
            {
                await Application.Current.Windows[0].Page!.DisplayAlert(
                    "Change Profile Picture",
                    "Image selection will be implemented in a future update",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing profile picture");
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await _navigationService.GoBackAsync();
    }

    private void PopulateFromDto(UserProfileDto dto)
    {
        UserId = dto.UserId;
        Username = dto.Username;
        Email = dto.Email;
        IsEmailVerified = dto.IsEmailVerified;
        PhoneNumber = dto.PhoneNumber ?? string.Empty;
        IsPhoneVerified = dto.IsPhoneVerified;
        FirstName = dto.FirstName ?? string.Empty;
        LastName = dto.LastName ?? string.Empty;
        ProfilePictureUrl = dto.ProfilePictureUrl ?? string.Empty;
        DateOfBirth = dto.DateOfBirth;
        Gender = dto.Gender ?? string.Empty;
        Country = dto.Country ?? string.Empty;
        Language = dto.Language ?? string.Empty;
        TimeZone = dto.TimeZone ?? string.Empty;
        LastLoginAt = dto.LastLoginAt;

        // Compute display name
        if (!string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName))
        {
            DisplayName = $"{FirstName} {LastName}";
            Initials = $"{FirstName[0]}{LastName[0]}".ToUpper();
        }
        else if (!string.IsNullOrWhiteSpace(FirstName))
        {
            DisplayName = FirstName;
            Initials = FirstName[0].ToString().ToUpper();
        }
        else if (!string.IsNullOrWhiteSpace(LastName))
        {
            DisplayName = LastName;
            Initials = LastName[0].ToString().ToUpper();
        }
        else
        {
            DisplayName = Username;
            Initials = Username.Length > 0 ? Username[0].ToString().ToUpper() : "?";
        }
    }
}
