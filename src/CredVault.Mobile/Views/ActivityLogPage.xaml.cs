using CredVault.Mobile.Helpers;
using CredVault.Mobile.ViewModels;

namespace CredVault.Mobile.Views;

public partial class ActivityLogPage : ContentPage
{
    private readonly ActivityLogViewModel _viewModel;

    public ActivityLogPage()
        : this(ServiceHelper.GetRequiredService<ActivityLogViewModel>())
    {
    }

    public ActivityLogPage(ActivityLogViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        await _viewModel.InitializeAsync();
    }

    private async void OnFilterChanged(object sender, EventArgs e)
    {
        await _viewModel.FilterChangedCommand.ExecuteAsync(null);
    }
}
