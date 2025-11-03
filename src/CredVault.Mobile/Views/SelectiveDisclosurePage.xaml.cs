using CredVault.Mobile.Helpers;
using CredVault.Mobile.ViewModels;

namespace CredVault.Mobile.Views;

public partial class SelectiveDisclosurePage : ContentPage
{
    public SelectiveDisclosurePage()
        : this(ServiceHelper.GetRequiredService<PresentationViewModel>())
    {
    }

    public SelectiveDisclosurePage(PresentationViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        InitializeComponent();
        BindingContext = viewModel;
    }
}
