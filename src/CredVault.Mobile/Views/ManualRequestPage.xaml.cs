using CredVault.Mobile.Helpers;
using CredVault.Mobile.ViewModels;

namespace CredVault.Mobile.Views;

public partial class ManualRequestPage : ContentPage
{
    private readonly ManualRequestViewModel _viewModel;

    public ManualRequestPage()
        : this(ServiceHelper.GetRequiredService<ManualRequestViewModel>())
    {
    }

    public ManualRequestPage(ManualRequestViewModel viewModel)
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

    private void OnClaimTapped(object sender, EventArgs e)
    {
        if (sender is Grid grid && grid.BindingContext is RequestClaim claim)
        {
            claim.IsSelected = !claim.IsSelected;
        }
    }
}
