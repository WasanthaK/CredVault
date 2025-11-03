using CredVault.Mobile.Helpers;
using CredVault.Mobile.ViewModels;

namespace CredVault.Mobile.Views;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;

    public SettingsPage()
        : this(ServiceHelper.GetRequiredService<SettingsViewModel>())
    {
    }

    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        await _viewModel.InitializeAsync();
    }

    private void OnThemeChanged(object sender, EventArgs e)
    {
        if (_viewModel.ThemeChangedCommand.CanExecute(null))
            _viewModel.ThemeChangedCommand.Execute(null);
    }

    private void OnLanguageChanged(object sender, EventArgs e)
    {
        if (_viewModel.LanguageChangedCommand.CanExecute(null))
            _viewModel.LanguageChangedCommand.Execute(null);
    }

    private void OnNotificationsToggled(object sender, ToggledEventArgs e)
    {
        if (_viewModel.NotificationsToggledCommand.CanExecute(null))
            _viewModel.NotificationsToggledCommand.Execute(null);
    }

    private void OnBiometricToggled(object sender, ToggledEventArgs e)
    {
        if (_viewModel.BiometricToggledCommand.CanExecute(null))
            _viewModel.BiometricToggledCommand.Execute(null);
    }

    private async void OnPinToggled(object sender, ToggledEventArgs e)
    {
        if (_viewModel.PinToggledCommand.CanExecute(null))
            await _viewModel.PinToggledCommand.ExecuteAsync(null);
    }

    private void OnAutoLockToggled(object sender, ToggledEventArgs e)
    {
        if (_viewModel.AutoLockToggledCommand.CanExecute(null))
            _viewModel.AutoLockToggledCommand.Execute(null);
    }

    private async void OnTermsTapped(object sender, EventArgs e)
    {
        await DisplayAlert("Terms of Service", "Terms of Service content will be displayed here.", "OK");
    }

    private async void OnPrivacyTapped(object sender, EventArgs e)
    {
        await DisplayAlert("Privacy Policy", "Privacy Policy content will be displayed here.", "OK");
    }
}
