using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CredVault.Mobile.Models;
using CredVault.Mobile.Services;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace CredVault.Mobile.ViewModels;

public partial class AddCredentialViewModel : BaseViewModel
{
    private readonly WalletService _walletService;
    private readonly IdentityService _identityService;
    private readonly AuthenticationFlowService _authFlowService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<AddCredentialViewModel> _logger;

    // Step 1: Credential Type Selection
    [ObservableProperty]
    private ObservableCollection<CredentialTypeInfo> availableCredentialTypes = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ProceedToAuthenticationCommand))]
    [NotifyPropertyChangedFor(nameof(CanProceed))]
    private CredentialTypeInfo? selectedCredentialType = null;

    [ObservableProperty]
    private string selectedFilter = "All";

    [ObservableProperty]
    private ObservableCollection<string> credentialFilters = new() { "All", "IDs", "Insurance", "Education", "Health", "Travel" };

    // Step 2: Authentication
    [ObservableProperty]
    private string issuerName = string.Empty;

    [ObservableProperty]
    private string issuerLogoUrl = string.Empty;

    [ObservableProperty]
    private bool isAuthenticating;

    [ObservableProperty]
    private int authenticationProgress;

    // Step 3: Consent & Review
    [ObservableProperty]
    private ObservableCollection<CredentialClaimInfo> claimsToIssue = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmIssuanceCommand))]
    private bool hasReviewedClaims;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmIssuanceCommand))]
    private bool hasConsentedToIssuance;

    // Step 4: Confirmation
    [ObservableProperty]
    private bool credentialAddedSuccessfully;

    [ObservableProperty]
    private string? newCredentialId;

    // Current step tracking
    [ObservableProperty]
    private int currentStep = 1;

    [ObservableProperty]
    private int totalSteps = 3;

    // Store credential offer details
    private CredentialOfferDetails? _credentialOffer;

    public AddCredentialViewModel(
        WalletService walletService,
        IdentityService identityService,
        AuthenticationFlowService authFlowService,
        INavigationService navigationService,
        ILogger<AddCredentialViewModel> logger)
    {
        _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        _authFlowService = authFlowService ?? throw new ArgumentNullException(nameof(authFlowService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Title = "Add Credential";

        PropertyChanged += OnViewModelPropertyChanged;
    }

    private void NotifyCommandStates()
    {
        ProceedToAuthenticationCommand.NotifyCanExecuteChanged();
        ConfirmIssuanceCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            NotifyCommandStates();
            await LoadAvailableCredentialTypesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing AddCredentialViewModel");
            await ShowAlertAsync("Error", "Failed to load credential types", "OK");
        }
        finally
        {
            IsBusy = false;
            NotifyCommandStates();
        }
    }

    private async Task LoadAvailableCredentialTypesAsync()
    {
        // TODO: Load from API - for now using mock data based on designs
        AvailableCredentialTypes.Clear();

        var types = new List<CredentialTypeInfo>
        {
            new CredentialTypeInfo
            {
                Type = "NationalID",
                DisplayName = "National ID",
                IssuerName = "Gov ID Authority",
                IssuerId = "gov-id-authority",
                IssuerLogoUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuAkdCdwdTQS2LSF_HzQjIWaw7LmQxOYdPCDbDRQbjCBVHvJghcCASWdfsU03XAoCTx_3wG6JZbPtj9QU_AujE8-mUYQpsyZQy398bQ9B6I93n6GT-m5b_6Gjnyk1D3NXqnm6zal8DzCcsNzqdzjia40AJiRSLSrjPbv7mDMO3MAXec85pvuEsSKjyKXWBDQJH0XTD910LYmfUpRq4hZntniPqzcfP3FiI2r2QDAndslPNhNflZXAC7MryyWCQI5N6z6ZBA9tjBJnUwZ",
                Category = "IDs",
                IsPopular = true
            },
            new CredentialTypeInfo
            {
                Type = "DriversLicense",
                DisplayName = "Driver's License",
                IssuerName = "Dept. of Motor Vehicles",
                IssuerId = "dmv-authority",
                IssuerLogoUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuBie6msvr8hGgr7wmlu_qeMCACAKXzfoBqHN7F8_ZBxe9STLjl7qD-k_HFeMc-Qi5C2ed4e1fHgonYUPUJ5hch3JFzl21G9WjGR0m81V06sNlG8kWCbEHzyFDeqMRuais15n8Mu39ajQyIPcfGksyUVxuFP8-CfFLNlHkKz9EqlS5i0wwC0RIsJTy4zG08DsSj105lYKs9XdJv_sLoVVa0hp9bS7uj-5621GimtgAFqUcNEqfhIwXvRQaX6AHkwXLDWjxJIU5oxuJW7",
                Category = "IDs"
            },
            new CredentialTypeInfo
            {
                Type = "UniversityDiploma",
                DisplayName = "University Diploma",
                IssuerName = "State University",
                IssuerId = "state-university",
                IssuerLogoUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuD3JrENhtEe-Fjh4TJ-1McGI4y1R_YL3CqYhnn7urph2oVatEPfrpMoDAp9qzbrsXxY5tS3t4Jbzch57NH7xs-MMbbu7RX7P_wQVuBi1z5a2PWFgMPP1-Zqaf9vfn9-vOrx9nL0w4EiKQVcJpSMOnp9E5af30oaqU70gfxkRvFbS-GHUEgPee1KUPh4SoIjXsD0UqGWCvK_mAQGOMcOoyKTR0vEOUrUmU1ps92UvhmQK7UWCfKjZAt5T_T2pmkcwG90i6V79n_dxnHW",
                Category = "Education"
            }
        };

        foreach (var type in types)
        {
            AvailableCredentialTypes.Add(type);
        }

        await Task.CompletedTask;
    }

    [RelayCommand]
    private void SelectCredentialType(CredentialTypeInfo credentialType)
    {
        if (credentialType == null) return;

        SelectedCredentialType = credentialType;
        _logger.LogInformation("Selected credential type: {Type}", credentialType.Type);
    }

    [RelayCommand]
    private void FilterCredentials(string filter)
    {
        SelectedFilter = filter;
        _logger.LogInformation("Applied filter: {Filter}", filter);
        // TODO: Implement filtering logic
    }

    [RelayCommand(CanExecute = nameof(CanProceedToAuthentication))]
    private async Task ProceedToAuthenticationAsync()
    {
        if (SelectedCredentialType == null) return;

        try
        {
            IsBusy = true;
            NotifyCommandStates();
            CurrentStep = 2;
            IssuerName = SelectedCredentialType.IssuerName;
            IssuerLogoUrl = SelectedCredentialType.IssuerLogoUrl;

            // Start REAL authentication flow
            await StartRealAuthenticationFlowAsync();
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("User cancelled authentication");
            await ShowAlertAsync("Cancelled", "Authentication was cancelled. You can try again.", "OK");
            CurrentStep = 1; // Go back to selection
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proceeding to authentication");
            await ShowAlertAsync("Error", $"Failed to start authentication: {ex.Message}", "OK");
            CurrentStep = 1; // Go back to selection
        }
        finally
        {
            IsBusy = false;
            NotifyCommandStates();
        }
    }

    private bool CanProceedToAuthentication() => SelectedCredentialType != null && !IsBusy;

    public bool CanProceed => SelectedCredentialType != null && !IsBusy;

    partial void OnSelectedCredentialTypeChanged(CredentialTypeInfo? value)
    {
        NotifyCommandStates();
        OnPropertyChanged(nameof(CanProceed));
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IsBusy))
        {
            NotifyCommandStates();
            OnPropertyChanged(nameof(CanProceed));
        }
    }

    private async Task StartRealAuthenticationFlowAsync()
    {
        if (SelectedCredentialType == null) return;

        IsAuthenticating = true;
        AuthenticationProgress = 0;

        try
        {
            _logger.LogInformation("Starting REAL OAuth authentication for {CredentialType}", SelectedCredentialType.Type);
            
            // Step 1: Get authorization URL from AuthenticationFlowService (10%)
            AuthenticationProgress = 10;
            var authUrlResult = await _authFlowService.StartCredentialIssuanceFlowAsync(
                SelectedCredentialType.Type,
                SelectedCredentialType.IssuerId ?? "default-issuer"
            );

            if (!authUrlResult.IsSuccess)
            {
                throw new Exception(authUrlResult.ErrorMessage ?? "Failed to get authorization URL");
            }

            _logger.LogInformation("Authorization URL obtained: {AuthUrl}", authUrlResult.Data);
            
            // Step 2: Launch WebAuthenticator (Browser opens) (30%)
            AuthenticationProgress = 30;
            
#if ANDROID || IOS || MACCATALYST
            // Use platform WebAuthenticator
            var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new WebAuthenticatorOptions
                {
                    Url = new Uri(authUrlResult.Data!),
                    CallbackUrl = new Uri("credvault://oauth-callback"),
                    PrefersEphemeralWebBrowserSession = true
                });

            // Extract authorization code
            var authCode = authResult.Properties.ContainsKey("code") 
                ? authResult.Properties["code"] 
                : throw new Exception("No authorization code received");
            
            var state = authResult.Properties.ContainsKey("state") 
                ? authResult.Properties["state"] 
                : string.Empty;
#else
            // For Windows/Desktop: Simulate OAuth for now
            _logger.LogWarning("WebAuthenticator not available on this platform. Using simulation.");
            await Task.Delay(2000); // Simulate user authentication
            var authCode = "simulated_auth_code_" + Guid.NewGuid().ToString("N");
            var state = "simulated_state";
#endif

            // Step 3: Exchange authorization code for access token (50%)
            AuthenticationProgress = 50;
            _logger.LogInformation("Exchanging authorization code for access token");
            
            var tokenResult = await _authFlowService.HandleOAuthCallbackAsync(authCode, state);
            
            if (!tokenResult.IsSuccess)
            {
                throw new Exception(tokenResult.ErrorMessage ?? "Failed to exchange authorization code");
            }

            _logger.LogInformation("Access token obtained successfully");
            
            // Step 4: Request credential offer with real claims (70%)
            AuthenticationProgress = 70;
            _logger.LogInformation("Requesting credential offer from issuer");
            
            var offerResult = await _authFlowService.RequestCredentialIssuanceAsync(
                SelectedCredentialType.Type,
                SelectedCredentialType.IssuerId ?? "default-issuer"
            );
            
            if (!offerResult.IsSuccess)
            {
                throw new Exception(offerResult.ErrorMessage ?? "Failed to get credential offer");
            }

            _logger.LogInformation("Credential offer received with {ClaimCount} claims", 
                offerResult.Data?.Claims.Count ?? 0);
            
            // Store the credential offer
            _credentialOffer = offerResult.Data;
            
            // Step 5: Prepare consent UI with REAL claims (90%)
            AuthenticationProgress = 90;
            await LoadCredentialClaimsFromOfferAsync(offerResult.Data!);
            
            // Step 6: Complete authentication (100%)
            AuthenticationProgress = 100;
            await Task.Delay(300);
            
            _logger.LogInformation("Authentication flow completed successfully");
            await ProceedToConsentReviewAsync();
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Authentication cancelled by user");
            throw; // Re-throw to be handled by caller
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication failed");
            throw; // Re-throw to be handled by caller
        }
        finally
        {
            IsAuthenticating = false;
            AuthenticationProgress = 0;
        }
    }

    private async Task LoadCredentialClaimsFromOfferAsync(CredentialOfferDetails offer)
    {
        ClaimsToIssue.Clear();

        // Convert offer claims to UI model
        foreach (var claim in offer.Claims)
        {
            var claimInfo = new CredentialClaimInfo
            {
                ClaimType = claim.Key,
                DisplayName = FormatClaimName(claim.Key),
                Value = claim.Value?.ToString() ?? string.Empty,
                IsRequired = true,
                Icon = GetIconForClaim(claim.Key)
            };

            ClaimsToIssue.Add(claimInfo);
        }

        _logger.LogInformation("Loaded {ClaimCount} claims for consent review", ClaimsToIssue.Count);
    }

    private static string FormatClaimName(string claimType)
    {
        // Convert camelCase/PascalCase to Title Case with spaces
        return claimType switch
        {
            "fullName" => "Full Name",
            "dateOfBirth" => "Date of Birth",
            "idNumber" => "ID Number",
            "licenseNumber" => "License Number",
            "nationality" => "Nationality",
            "graduationDate" => "Graduation Date",
            "photo" => "Photo",
            "degree" => "Degree",
            "major" => "Major",
            "gpa" => "GPA",
            "honors" => "Honors",
            "class" => "License Class",
            "restrictions" => "Restrictions",
            _ => System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                System.Text.RegularExpressions.Regex.Replace(claimType, "([a-z])([A-Z])", "$1 $2"))
        };
    }

    private static string GetIconForClaim(string claimType)
    {
        return claimType.ToLower() switch
        {
            "fullname" or "name" => "👤",
            "dateofbirth" or "birthdate" => "🎂",
            "idnumber" or "licensenumber" => "🔢",
            "photo" => "📷",
            "nationality" => "🌍",
            "degree" or "diploma" => "🎓",
            "major" or "field" => "📚",
            "gpa" or "grade" => "📊",
            "honors" => "🏆",
            "class" => "🚗",
            "restrictions" => "⚠️",
            "graduationdate" => "📅",
            _ => "📄"
        };
    }

    [RelayCommand]
    private async Task ProceedToConsentReviewAsync()
    {
        try
        {
            IsBusy = true;
            NotifyCommandStates();
            CurrentStep = 3;

            // Load claims that will be included in the credential
            await LoadCredentialClaimsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading credential claims");
            await ShowAlertAsync("Error", "Failed to load credential information", "OK");
        }
        finally
        {
            IsBusy = false;
            NotifyCommandStates();
        }
    }

    private async Task LoadCredentialClaimsAsync()
    {
        // TODO: Load actual claims from authentication result
        ClaimsToIssue.Clear();

        var mockClaims = new List<CredentialClaimInfo>
        {
            new CredentialClaimInfo { Icon = "person", Label = "Full Name", Value = "Jane Doe" },
            new CredentialClaimInfo { Icon = "cake", Label = "Date of Birth", Value = "August 12, 1990" },
            new CredentialClaimInfo { Icon = "image", Label = "Photo", Value = "ID Photo" },
            new CredentialClaimInfo { Icon = "fingerprint", Label = "ID Number", Value = "ID-123456789" },
            new CredentialClaimInfo { Icon = "location_on", Label = "Address", Value = "123 Main St, City, Country" }
        };

        foreach (var claim in mockClaims)
        {
            ClaimsToIssue.Add(claim);
        }

        await Task.CompletedTask;
    }

    [RelayCommand]
    private void ToggleReviewed()
    {
        HasReviewedClaims = !HasReviewedClaims;
    }

    [RelayCommand]
    private void ToggleConsent()
    {
        HasConsentedToIssuance = !HasConsentedToIssuance;
    }

    [RelayCommand(CanExecute = nameof(CanConfirmIssuance))]
    private async Task ConfirmIssuanceAsync()
    {
        if (!HasReviewedClaims || !HasConsentedToIssuance)
        {
            await ShowAlertAsync("Required", "Please review and consent to the credential issuance", "OK");
            return;
        }

        if (_credentialOffer == null)
        {
            await ShowAlertAsync("Error", "Credential offer is missing. Please start over.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            NotifyCommandStates();
            Title = "Issuing Credential...";

            _logger.LogInformation("User confirmed issuance consent for {CredentialType}", _credentialOffer.CredentialType);

            // Use AuthenticationFlowService to accept and store the credential
            var result = await _authFlowService.AcceptAndStoreCredentialAsync(
                _credentialOffer,
                userConsented: true
            );

            if (result.IsSuccess && result.Data != null)
            {
                _logger.LogInformation("Credential issued successfully: {CredentialId}", result.Data.Id);
                CredentialAddedSuccessfully = true;
                NewCredentialId = result.Data.Id;
                await ShowConfirmationAsync();
            }
            else
            {
                _logger.LogError("Failed to accept and store credential: {Message}", result.ErrorMessage);
                await ShowAlertAsync("Error", result.ErrorMessage ?? "Failed to add credential", "OK");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming credential issuance");
            await ShowAlertAsync("Error", "An unexpected error occurred while adding the credential", "OK");
        }
        finally
        {
            IsBusy = false;
            NotifyCommandStates();
        }
    }

    private bool CanConfirmIssuance() => HasReviewedClaims && HasConsentedToIssuance && !IsBusy;

    /// <summary>
    /// Issue credential using OpenID4VCI protocol
    /// </summary>
    private async Task ShowConfirmationAsync()
    {
        // Navigate to confirmation page or show success state
        await Task.Delay(2000); // Show success message
        await NavigateToWalletAsync();
    }

    [RelayCommand]
    private async Task NavigateToWalletAsync()
    {
        try
        {
            await _navigationService.NavigateToAsync("//credentials");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to wallet");
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        var confirm = await ShowConfirmAsync(
            "Cancel",
            "Are you sure you want to cancel adding this credential?",
            "Yes",
            "No");

        if (confirm)
        {
            await _navigationService.GoBackAsync();
        }
    }

    private async Task ShowAlertAsync(string title, string message, string button)
    {
        var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (mainPage != null)
        {
            await mainPage.DisplayAlert(title, message, button);
        }
    }

    private async Task<bool> ShowConfirmAsync(string title, string message, string accept, string cancel)
    {
        var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (mainPage != null)
        {
            return await mainPage.DisplayAlert(title, message, accept, cancel);
        }
        return false;
    }
}

// Supporting models for the add credential flow

public class CredentialTypeInfo
{
    public string Type { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string IssuerName { get; set; } = string.Empty;
    public string IssuerId { get; set; } = string.Empty;
    public string IssuerLogoUrl { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsPopular { get; set; }
}

public class CredentialClaimInfo
{
    public string ClaimType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = true;
}
