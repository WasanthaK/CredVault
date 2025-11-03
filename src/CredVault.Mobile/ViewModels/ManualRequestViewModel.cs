using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CredVault.Mobile.Services;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace CredVault.Mobile.ViewModels;

public partial class ManualRequestViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly ILogger<ManualRequestViewModel> _logger;

    [ObservableProperty]
    private string title = "Request Information";

    [ObservableProperty]
    private ObservableCollection<RequestClaim> availableClaims = new();

    [ObservableProperty]
    private string? generatedQrData;

    [ObservableProperty]
    private bool showQrModal;

    public ManualRequestViewModel(
        INavigationService navigationService,
        ILogger<ManualRequestViewModel> logger)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        InitializeAvailableClaims();
    }

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task GenerateQrCodeAsync()
    {
        try
        {
            var selectedClaims = AvailableClaims.Where(c => c.IsSelected).ToList();

            if (!selectedClaims.Any())
            {
                if (Application.Current?.Windows.Count > 0)
                {
                    await Application.Current.Windows[0].Page!.DisplayAlert(
                        "No Claims Selected",
                        "Please select at least one claim to request.",
                        "OK");
                }
                return;
            }

            _logger.LogInformation("Generating QR code for {Count} selected claims", selectedClaims.Count);

            // In production, this would create an OpenID4VP authorization request
            // with the requested claims and encode it as a QR code
            var requestData = $"openid4vp://request?claims={string.Join(",", selectedClaims.Select(c => c.Name))}";
            
            GeneratedQrData = requestData;
            ShowQrModal = true;

            _logger.LogInformation("QR code generated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating QR code");
            if (Application.Current?.Windows.Count > 0)
            {
                await Application.Current.Windows[0].Page!.DisplayAlert(
                    "Error",
                    "Failed to generate QR code",
                    "OK");
            }
        }
    }

    [RelayCommand]
    private void CloseQrModal()
    {
        ShowQrModal = false;
        GeneratedQrData = null;
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await _navigationService.GoBackAsync();
    }

    private void InitializeAvailableClaims()
    {
        AvailableClaims = new ObservableCollection<RequestClaim>
        {
            new RequestClaim { Name = "Full Name", IsRequired = true },
            new RequestClaim { Name = "Date of Birth", IsRequired = false },
            new RequestClaim { Name = "Address", IsRequired = false },
            new RequestClaim { Name = "Age Over 18", IsRequired = false },
            new RequestClaim { Name = "Age Over 21", IsRequired = false },
            new RequestClaim { Name = "Nationality", IsRequired = false },
            new RequestClaim { Name = "Email Address", IsRequired = false },
            new RequestClaim { Name = "Phone Number", IsRequired = false }
        };
    }

    partial void OnAvailableClaimsChanged(ObservableCollection<RequestClaim> value)
    {
        // Subscribe to property changes of all claims
        foreach (var claim in value)
        {
            claim.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(RequestClaim.IsSelected))
                {
                    OnPropertyChanged(nameof(AvailableClaims));
                }
            };
        }
    }
}

public partial class RequestClaim : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private bool isSelected;

    [ObservableProperty]
    private bool isRequired;
}
