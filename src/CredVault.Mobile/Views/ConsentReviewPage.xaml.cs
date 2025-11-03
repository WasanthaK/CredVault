using CredVault.Mobile.Helpers;
using CredVault.Mobile.ViewModels;

namespace CredVault.Mobile.Views;

public partial class ConsentReviewPage : ContentPage
{
    public ConsentReviewPage()
        : this(ServiceHelper.GetRequiredService<AddCredentialViewModel>())
    {
    }

    public ConsentReviewPage(AddCredentialViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        InitializeComponent();
        BindingContext = viewModel;
    }
}
