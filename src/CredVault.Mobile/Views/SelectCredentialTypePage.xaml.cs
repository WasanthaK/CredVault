using CredVault.Mobile.Helpers;
using CredVault.Mobile.ViewModels;

namespace CredVault.Mobile.Views;

public partial class SelectCredentialTypePage : ContentPage
{
    private readonly AddCredentialViewModel _viewModel;

    public SelectCredentialTypePage()
        : this(ServiceHelper.GetRequiredService<AddCredentialViewModel>())
    {
    }

    public SelectCredentialTypePage(AddCredentialViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        await _viewModel.InitializeCommand.ExecuteAsync(null);
    }
}
