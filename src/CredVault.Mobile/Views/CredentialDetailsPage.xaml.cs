using CredVault.Mobile.Helpers;
using CredVault.Mobile.ViewModels;

namespace CredVault.Mobile.Views;

public partial class CredentialDetailsPage : ContentPage
{
    private readonly CredentialDetailsViewModel _viewModel;

    public CredentialDetailsPage()
        : this(ServiceHelper.GetRequiredService<CredentialDetailsViewModel>())
    {
    }

    public CredentialDetailsPage(CredentialDetailsViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Get credential ID from query parameters
        if (BindingContext is CredentialDetailsViewModel viewModel)
        {
            // Check if we have a credential ID passed via navigation parameters
            // This will be set by the navigation service when navigating from the list
            var credentialId = GetCredentialIdFromParameters();
            
            if (!string.IsNullOrEmpty(credentialId))
            {
                await viewModel.InitializeAsync(credentialId);
            }
        }
    }

    private string? GetCredentialIdFromParameters()
    {
        // Try to get from Shell query parameters
        if (Shell.Current != null)
        {
            var credentialId = Shell.Current.CurrentState?.Location?.OriginalString;
            // Parse query string for credential_id parameter
            // This is a simplified version - Shell handles this automatically with QueryProperty
        }

        return null;
    }
}
