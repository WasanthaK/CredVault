using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CredVault.Mobile.Models;
using CredVault.Mobile.Services;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace CredVault.Mobile.ViewModels;

public partial class CredentialsListViewModel : BaseViewModel
{
    private readonly WalletService _walletService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<CredentialsListViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<CredentialResponseDto> credentials = new();

    [ObservableProperty]
    private ObservableCollection<CredentialResponseDto> filteredCredentials = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string selectedFilter = "All";

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private int totalCredentials;

    [ObservableProperty]
    private int activeCredentials;

    [ObservableProperty]
    private int expiredCredentials;

    public CredentialsListViewModel(
        WalletService walletService,
        INavigationService navigationService,
        ILogger<CredentialsListViewModel> logger)
    {
        _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Title = "My Credentials";
    }

    [RelayCommand]
    private async Task LoadCredentialsAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            HasError = false;
            ErrorMessage = string.Empty;

            _logger.LogInformation("Loading credentials...");

            var result = await _walletService.GetCredentialsAsync();

            if (result.IsSuccess && result.Data != null)
            {
                Credentials.Clear();
                foreach (var credential in result.Data)
                {
                    Credentials.Add(credential);
                }

                UpdateStatistics();
                ApplyFilters();

                _logger.LogInformation("Loaded {Count} credentials", Credentials.Count);
            }
            else
            {
                HasError = true;
                ErrorMessage = result.ErrorMessage ?? "Failed to load credentials";
                _logger.LogWarning("Failed to load credentials: {Error}", ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = "An unexpected error occurred";
            _logger.LogError(ex, "Error loading credentials");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task RefreshCredentialsAsync()
    {
        IsRefreshing = true;
        await LoadCredentialsAsync();
    }

    [RelayCommand]
    private async Task ViewCredentialDetailsAsync(CredentialResponseDto credential)
    {
        if (credential == null)
            return;

        try
        {
            _logger.LogInformation("Navigating to credential details: {CredentialId}", credential.CredentialId);

            var parameters = new Dictionary<string, object>
            {
                { "CredentialId", credential.CredentialId ?? string.Empty }
            };

            await _navigationService.NavigateToAsync("credentialdetails", parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to credential details");
            ErrorMessage = "Failed to open credential details";
            HasError = true;
        }
    }

    [RelayCommand]
    private async Task AddCredentialAsync()
    {
        try
        {
            _logger.LogInformation("Navigating to add credential");
            await _navigationService.NavigateToAsync("addcredential");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to add credential");
            ErrorMessage = "Failed to open add credential page";
            HasError = true;
        }
    }

    [RelayCommand]
    private async Task FilterCredentialsAsync(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
            filter = "All";

        SelectedFilter = filter;
        _logger.LogDebug("Filtering credentials by: {Filter}", filter);

        await Task.Run(() => ApplyFilters());
    }

    [RelayCommand]
    private async Task SearchCredentialsAsync()
    {
        await Task.Run(() => ApplyFilters());
    }

    partial void OnSearchTextChanged(string value)
    {
        // Debounce search - could add delay here
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = Credentials.AsEnumerable();

        // Apply status filter
        if (SelectedFilter != "All")
        {
            filtered = SelectedFilter switch
            {
                "Active" => filtered.Where(c => c.Status == CredentialStatus.Active),
                "Expired" => filtered.Where(c => c.Status == CredentialStatus.Expired),
                "Revoked" => filtered.Where(c => c.Status == CredentialStatus.Revoked),
                "Suspended" => filtered.Where(c => c.Status == CredentialStatus.Suspended),
                _ => filtered
            };
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLowerInvariant();
            filtered = filtered.Where(c =>
                (c.Type?.ToLowerInvariant().Contains(search) ?? false) ||
                (c.Issuer?.ToLowerInvariant().Contains(search) ?? false) ||
                (c.CredentialId?.ToLowerInvariant().Contains(search) ?? false));
        }

        // Update filtered collection
        MainThread.BeginInvokeOnMainThread(() =>
        {
            FilteredCredentials.Clear();
            foreach (var credential in filtered)
            {
                FilteredCredentials.Add(credential);
            }
        });
    }

    private void UpdateStatistics()
    {
        TotalCredentials = Credentials.Count;
        ActiveCredentials = Credentials.Count(c => c.Status == CredentialStatus.Active);
        ExpiredCredentials = Credentials.Count(c => c.Status == CredentialStatus.Expired);
    }

    public async Task InitializeAsync()
    {
        await LoadCredentialsAsync();
    }
}
