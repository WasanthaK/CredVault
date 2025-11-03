using CredVault.Mobile.Helpers;
using CredVault.Mobile.ViewModels;

namespace CredVault.Mobile.Views;

public partial class AuthenticateCredentialPage : ContentPage
{
    public AuthenticateCredentialPage()
        : this(ServiceHelper.GetRequiredService<AddCredentialViewModel>())
    {
    }

    public AuthenticateCredentialPage(AddCredentialViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        InitializeComponent();
        BindingContext = viewModel;
    }
}
