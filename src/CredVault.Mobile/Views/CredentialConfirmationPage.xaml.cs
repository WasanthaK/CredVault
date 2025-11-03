using CredVault.Mobile.Helpers;
using CredVault.Mobile.ViewModels;

namespace CredVault.Mobile.Views;

public partial class CredentialConfirmationPage : ContentPage
{
    public CredentialConfirmationPage()
        : this(ServiceHelper.GetRequiredService<AddCredentialViewModel>())
    {
    }

    public CredentialConfirmationPage(AddCredentialViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        InitializeComponent();
        BindingContext = viewModel;
    }
}
