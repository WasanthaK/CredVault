using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CredVault.Mobile.Models;
using CredVault.Mobile.Services;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace CredVault.Mobile.ViewModels;

public partial class PresentationViewModel : BaseViewModel
{
    private readonly WalletService _walletService;
    private readonly ConsentService _consentService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<PresentationViewModel> _logger;

    [ObservableProperty]
    private string verifierName = string.Empty;

    [ObservableProperty]
    private string verifierIcon = "verified_user";

    [ObservableProperty]
    private string verifierPurpose = string.Empty;

    [ObservableProperty]
    private ObservableCollection<RequestedCredentialInfo> requestedCredentials = new();

    [ObservableProperty]
    private ObservableCollection<PresentationClaimInfo> dataToShare = new();

    [ObservableProperty]
    private bool hasConsented;

    [ObservableProperty]
    private bool isPresentationSuccessful;

    [ObservableProperty]
    private string presentationMessage = string.Empty;

    [ObservableProperty]
    private string presentationRequestData = string.Empty;

    public PresentationViewModel(
        WalletService walletService,
        ConsentService consentService,
        INavigationService navigationService,
        ILogger<PresentationViewModel> logger)
    {
        _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
        _consentService = consentService ?? throw new ArgumentNullException(nameof(consentService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Title = "Presentation Request";
    }

    public async Task InitializeAsync(string presentationRequest)
    {
        try
        {
            IsBusy = true;
            PresentationRequestData = presentationRequest;

            _logger.LogInformation("Initializing presentation request");

            // Parse presentation request (OpenID4VP format)
            await ParsePresentationRequestAsync(presentationRequest);

            // Load matching credentials from wallet
            await LoadMatchingCredentialsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing presentation request");
            await ShowAlertAsync("Error", "Failed to load presentation request. Please try again.", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ParsePresentationRequestAsync(string requestData)
    {
        // In a real implementation, this would parse OpenID4VP presentation_definition
        // For now, using mock data

        try
        {
            // Try to parse as JSON
            if (requestData.StartsWith("{") || requestData.Contains("presentation_definition"))
            {
                var jsonDoc = JsonDocument.Parse(requestData);
                
                // Extract verifier info
                if (jsonDoc.RootElement.TryGetProperty("client_id", out var clientId))
                {
                    VerifierName = clientId.GetString() ?? "Unknown Verifier";
                }
                
                if (jsonDoc.RootElement.TryGetProperty("purpose", out var purpose))
                {
                    VerifierPurpose = purpose.GetString() ?? "Credential verification";
                }
            }
            else
            {
                // Fallback to mock data
                VerifierName = "City Liquor Store";
                VerifierIcon = "storefront";
                VerifierPurpose = "requests proof that you are over 18";
            }

            _logger.LogInformation("Parsed presentation request from {VerifierName}", VerifierName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse presentation request, using defaults");
            
            // Use default values
            VerifierName = "Unknown Verifier";
            VerifierIcon = "verified_user";
            VerifierPurpose = "requests to verify your credentials";
        }

        await Task.CompletedTask;
    }

    private async Task LoadMatchingCredentialsAsync()
    {
        try
        {
            // In a real implementation, this would:
            // 1. Parse the presentation_definition to get required credential types
            // 2. Query wallet for matching credentials
            // 3. Filter credentials based on constraints

            // Mock implementation - load requested credentials
            RequestedCredentials.Clear();
            RequestedCredentials.Add(new RequestedCredentialInfo
            {
                CredentialType = "ProofOfAge",
                DisplayName = "Proof of Age",
                Icon = "badge",
                IsAvailable = true,
                CredentialId = "cred-123"
            });

            RequestedCredentials.Add(new RequestedCredentialInfo
            {
                CredentialType = "NationalID",
                DisplayName = "National ID",
                Icon = "fingerprint",
                IsAvailable = true,
                CredentialId = "cred-456"
            });

            // Load claims that will be shared (selective disclosure)
            DataToShare.Clear();
            DataToShare.Add(new PresentationClaimInfo
            {
                Label = "Age Over 18",
                Value = "Yes",
                Icon = "check_circle",
                IsRequired = true,
                WillBeShared = true
            });

            DataToShare.Add(new PresentationClaimInfo
            {
                Label = "Full Name",
                Value = "Hidden",
                Icon = "lock",
                IsRequired = false,
                WillBeShared = false
            });

            DataToShare.Add(new PresentationClaimInfo
            {
                Label = "Date of Birth",
                Value = "Hidden",
                Icon = "lock",
                IsRequired = false,
                WillBeShared = false
            });

            DataToShare.Add(new PresentationClaimInfo
            {
                Label = "Address",
                Value = "Hidden",
                Icon = "lock",
                IsRequired = false,
                WillBeShared = false
            });

            _logger.LogInformation("Loaded {Count} requested credentials", RequestedCredentials.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading matching credentials");
            throw;
        }

        await Task.CompletedTask;
    }

    [RelayCommand]
    private void ToggleConsent()
    {
        HasConsented = !HasConsented;
        _logger.LogInformation("Consent toggled: {HasConsented}", HasConsented);
    }

    [RelayCommand]
    private void ToggleClaim(PresentationClaimInfo claim)
    {
        if (claim != null && !claim.IsRequired)
        {
            claim.WillBeShared = !claim.WillBeShared;
            _logger.LogInformation("Claim {Label} toggled: {WillBeShared}", claim.Label, claim.WillBeShared);
        }
    }

    [RelayCommand]
    private async Task ReviewAndShareAsync()
    {
        // Navigate to selective disclosure preview page
    await _navigationService.NavigateToAsync("selectivedisclosure");
    }

    public bool CanApprove => HasConsented && RequestedCredentials.Any(c => c.IsAvailable);

    [RelayCommand(CanExecute = nameof(CanApprove))]
    private async Task ApproveAndPresentAsync()
    {
        try
        {
            IsBusy = true;
            _logger.LogInformation("Approving presentation request for {VerifierName}", VerifierName);

            // Record consent
            var consentRequest = new ConsentRequestDto
            {
                UserId = "user-123", // TODO: Get from IdentityService
                ResourceOwner = "user-123",
                ClientId = VerifierName,
                Scopes = DataToShare.Where(c => c.WillBeShared).Select(c => c.Label).ToList(),
                Purpose = $"Share credentials with {VerifierName}",
                ExpiresAt = DateTime.UtcNow.AddMinutes(5) // Short-lived consent for presentation
            };

            var consentResult = await _consentService.CreateConsentAsync(consentRequest);

            if (!consentResult.IsSuccess)
            {
                _logger.LogWarning("Consent creation failed");
                await ShowAlertAsync("Error", "Failed to record consent. Presentation cancelled.", "OK");
                return;
            }

            // Generate VP (Verifiable Presentation) using OpenID4VP
            var vpResult = await GenerateVerifiablePresentationAsync();

            if (vpResult.IsSuccess)
            {
                // Submit presentation to verifier
                var submitResult = await SubmitPresentationAsync(vpResult.Data!);

                if (submitResult.IsSuccess)
                {
                    IsPresentationSuccessful = true;
                    PresentationMessage = "Credentials shared successfully!";
                    
                    // Navigate to result page
                    await _navigationService.NavigateToAsync("presentationresult");
                }
                else
                {
                    await ShowAlertAsync("Error", submitResult.ErrorMessage ?? "Failed to submit presentation.", "OK");
                }
            }
            else
            {
                await ShowAlertAsync("Error", vpResult.ErrorMessage ?? "Failed to generate presentation.", "OK");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving presentation");
            await ShowAlertAsync("Error", "Failed to process presentation. Please try again.", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private Task<ServiceResult<string>> GenerateVerifiablePresentationAsync()
    {
        try
        {
            _logger.LogInformation("Generating Verifiable Presentation");

            // In a real implementation, this would:
            // 1. Retrieve selected credentials from wallet
            // 2. Apply selective disclosure (hide non-requested claims)
            // 3. Generate cryptographic proof (signature)
            // 4. Format as VP in OpenID4VP format

            // Mock VP generation
            var vp = new
            {
                type = new[] { "VerifiablePresentation" },
                verifiableCredential = RequestedCredentials
                    .Where(c => c.IsAvailable)
                    .Select(c => new
                    {
                        id = c.CredentialId,
                        type = c.CredentialType,
                        claims = DataToShare.Where(d => d.WillBeShared).Select(d => new { d.Label, d.Value })
                    }),
                proof = new
                {
                    type = "Ed25519Signature2020",
                    created = DateTime.UtcNow.ToString("O"),
                    verificationMethod = "did:example:holder#key-1",
                    proofPurpose = "authentication"
                }
            };

            var vpJson = JsonSerializer.Serialize(vp);
            
            _logger.LogInformation("Generated VP with {Count} credentials", RequestedCredentials.Count(c => c.IsAvailable));

            return Task.FromResult(ServiceResult<string>.Success(vpJson));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating VP");
            return Task.FromResult(ServiceResult<string>.Failure("Failed to generate verifiable presentation"));
        }
    }

    private async Task<ServiceResult<bool>> SubmitPresentationAsync(string vp)
    {
        try
        {
            _logger.LogInformation("Submitting presentation to {VerifierName}", VerifierName);

            // In a real implementation, this would:
            // 1. POST the VP to the verifier's callback URL
            // 2. Handle response (success/failure/redirect)
            // 3. Optionally wait for verification result

            // Mock submission - simulate network delay
            await Task.Delay(1500);

            // Log the presentation event (for activity log)
            _logger.LogInformation("Presentation submitted successfully");

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting presentation");
            return ServiceResult<bool>.Failure("Failed to submit presentation to verifier");
        }
    }

    [RelayCommand]
    private async Task DenyAsync()
    {
        var confirmed = await ShowConfirmAsync(
            "Deny Request",
            "Are you sure you want to deny this presentation request?",
            "Yes, Deny",
            "Cancel");

        if (confirmed)
        {
            _logger.LogInformation("User denied presentation request from {VerifierName}", VerifierName);
            await _navigationService.GoBackAsync();
        }
    }

    [RelayCommand]
    private async Task BackToWalletAsync()
    {
        await _navigationService.NavigateToAsync("//credentials");
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

/// <summary>
/// Information about a requested credential for presentation
/// </summary>
public class RequestedCredentialInfo
{
    public string CredentialType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string CredentialId { get; set; } = string.Empty;
}

/// <summary>
/// Information about a claim being shared in a presentation
/// </summary>
public partial class PresentationClaimInfo : ObservableObject
{
    [ObservableProperty]
    private string icon = string.Empty;
    
    [ObservableProperty]
    private string label = string.Empty;
    
    [ObservableProperty]
    private string value = string.Empty;
    
    [ObservableProperty]
    private bool isRequired;
    
    [ObservableProperty]
    private bool willBeShared;
    
    [ObservableProperty]
    private bool isNotLastItem = true;
}
