using CredVault.Mobile.Helpers;
using CredVault.Mobile.ViewModels;

namespace CredVault.Mobile.Views;

public partial class PresentationRequestPage : ContentPage
{
    private readonly PresentationViewModel _viewModel;

    public PresentationRequestPage()
        : this(ServiceHelper.GetRequiredService<PresentationViewModel>())
    {
    }

    public PresentationRequestPage(PresentationViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        
        // Get presentation request from query parameters
        var presentationRequest = string.Empty;
        
        // Try to get from query string
        if (Uri.TryCreate(Shell.Current.CurrentState.Location.ToString(), UriKind.RelativeOrAbsolute, out var uri))
        {
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            presentationRequest = query["presentationRequest"] ?? string.Empty;
        }

        // Initialize with the request data
        await _viewModel.InitializeAsync(presentationRequest);
    }
}
