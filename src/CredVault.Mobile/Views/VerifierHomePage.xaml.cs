using CredVault.Mobile.Helpers;
using CredVault.Mobile.ViewModels;

namespace CredVault.Mobile.Views;

public partial class VerifierHomePage : ContentPage
{
    private readonly VerifierViewModel _viewModel;

    public VerifierHomePage()
        : this(ServiceHelper.GetRequiredService<VerifierViewModel>())
    {
    }

    public VerifierHomePage(VerifierViewModel viewModel)
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

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("settings");
    }

    private async void OnHelpTapped(object sender, EventArgs e)
    {
        await DisplayAlert(
            "How to Verify",
            "1. Ask the holder to prepare their digital credential\n" +
            "2. Tap 'Scan Credential QR' to open the scanner\n" +
            "3. Point your camera at the holder's credential QR code\n" +
            "4. Wait for automatic verification\n" +
            "5. Review the verification result",
            "Got it");
    }
}
