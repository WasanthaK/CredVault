using CredVault.Mobile.Helpers;
using CredVault.Mobile.ViewModels;

namespace CredVault.Mobile.Views;

public partial class PinPage : ContentPage
{
    private readonly PinViewModel _viewModel;

    public PinPage()
        : this(ServiceHelper.GetRequiredService<PinViewModel>())
    {
    }

    public PinPage(PinViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;
    }
}
