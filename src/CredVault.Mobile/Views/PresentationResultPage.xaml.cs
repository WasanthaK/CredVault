using CredVault.Mobile.Helpers;
using CredVault.Mobile.ViewModels;

namespace CredVault.Mobile.Views;

public partial class PresentationResultPage : ContentPage
{
    public PresentationResultPage()
        : this(ServiceHelper.GetRequiredService<PresentationViewModel>())
    {
    }

    public PresentationResultPage(PresentationViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        InitializeComponent();
        BindingContext = viewModel;
    }
}
