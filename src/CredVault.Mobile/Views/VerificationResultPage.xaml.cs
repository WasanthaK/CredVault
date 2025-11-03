using CredVault.Mobile.Helpers;
using CredVault.Mobile.ViewModels;

namespace CredVault.Mobile.Views;

public partial class VerificationResultPage : ContentPage
{
    private readonly VerifierViewModel _viewModel;

    public VerificationResultPage()
        : this(ServiceHelper.GetRequiredService<VerifierViewModel>())
    {
    }

    public VerificationResultPage(VerifierViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;
    }
}
