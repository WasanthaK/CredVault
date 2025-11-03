using CredVault.Mobile.Helpers;
using CredVault.Mobile.ViewModels;
using ZXing.Net.Maui;

namespace CredVault.Mobile.Views;

public partial class QrScannerPage : ContentPage, IQueryAttributable
{
    private readonly QrScannerViewModel _viewModel;
    private readonly VerifierViewModel? _verifierViewModel;

    public QrScannerPage()
        : this(ServiceHelper.GetRequiredService<QrScannerViewModel>(), ServiceHelper.GetRequiredService<VerifierViewModel>())
    {
    }

    public QrScannerPage(QrScannerViewModel viewModel, VerifierViewModel verifierViewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(verifierViewModel);
        InitializeComponent();
        _viewModel = viewModel;
        _verifierViewModel = verifierViewModel;
        BindingContext = _viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("mode", out var modeObj) && modeObj is string modeStr)
        {
            if (Enum.TryParse<ScanMode>(modeStr, out var mode))
            {
                _viewModel.SetScanMode(mode);
                
                if (mode == ScanMode.VerifierScan)
                {
                    _viewModel.VerifierViewModel = _verifierViewModel;
                }
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Request camera permissions
        var status = await Permissions.RequestAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            await DisplayAlert("Camera Permission Required", 
                "Please grant camera permission to scan QR codes.", 
                "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        // Enable scanning
        _viewModel.IsScanning = true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Disable scanning to release camera
        _viewModel.IsScanning = false;
    }

    private void CameraView_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        // Pass detected barcodes to ViewModel
        _viewModel.BarcodeDetectedCommand.Execute(e);
    }
}
