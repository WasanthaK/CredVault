using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CredVault.Mobile.Services;

namespace CredVault.Mobile.ViewModels;

public partial class SecuritySettingsViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private bool isBiometricsEnabled = true;

    [ObservableProperty]
    private bool requirePinForSharing;

    [ObservableProperty]
    private bool showSuccessMessage;

    [ObservableProperty]
    private string autoLockTimeoutText = "5 minutes";

    public SecuritySettingsViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    partial void OnIsBiometricsEnabledChanged(bool value)
    {
        ShowSuccessNotification();
    }

    partial void OnRequirePinForSharingChanged(bool value)
    {
        ShowSuccessNotification();
    }

    private async void ShowSuccessNotification()
    {
        ShowSuccessMessage = true;
        await Task.Delay(3000);
        ShowSuccessMessage = false;
    }

    [RelayCommand]
    private async Task GoBack()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task ChangePin()
    {
        await _navigationService.NavigateToAsync("securitypin");
    }

    [RelayCommand]
    private async Task SetAutoLock()
    {
        var result = await Shell.Current.DisplayActionSheet(
            "Auto-Lock Timeout",
            "Cancel",
            null,
            "Immediately",
            "1 minute",
            "5 minutes",
            "15 minutes",
            "30 minutes",
            "Never");

        if (result != null && result != "Cancel")
        {
            AutoLockTimeoutText = result;
            ShowSuccessNotification();
        }
    }
}
