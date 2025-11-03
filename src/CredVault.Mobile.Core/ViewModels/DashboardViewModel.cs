using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CredVault.Mobile.Core.Services;
using CredVault.Mobile.Core.Models;
using System.Collections.ObjectModel;

namespace CredVault.Mobile.Core.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IWalletService _walletService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string greetingText = "Good morning, User";

    [ObservableProperty]
    private ObservableCollection<CredentialCardInfo> credentials = new();

    [ObservableProperty]
    private ObservableCollection<ActivityInfo> recentActivities = new();

    [ObservableProperty]
    private bool hasRecentActivity;

    public DashboardViewModel(
        IWalletService walletService,
        INavigationService navigationService)
    {
        _walletService = walletService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    private async Task Initialize()
    {
        // Set greeting based on time of day
        var hour = DateTime.Now.Hour;
        var userName = "Alex"; // Get from user service later
        
        GreetingText = hour < 12 ? $"Good morning, {userName}" :
                      hour < 18 ? $"Good afternoon, {userName}" :
                      $"Good evening, {userName}";

        // Load credentials
        var result = await _walletService.GetCredentialsAsync();
        Credentials.Clear();
        
        if (result.IsSuccess && result.Data != null)
        {
            foreach (var cred in result.Data.Take(5))
            {
                Credentials.Add(new CredentialCardInfo
                {
                    Id = cred.Id ?? string.Empty,
                    Name = cred.Type ?? "Unknown Credential",
                    Icon = GetIconForType(cred.Type ?? ""),
                    ExpiryInfo = GetExpiryText(cred.ExpirationDate),
                    StatusColor = GetStatusColor(cred.Status.ToString())
                });
            }
        }

        // Load recent activities
        LoadRecentActivities();
    }

    private void LoadRecentActivities()
    {
        // Mock data - replace with actual service call
        RecentActivities.Clear();
        RecentActivities.Add(new ActivityInfo
        {
            Icon = "✅",
            Description = "National ID verified successfully",
            TimeAgo = "2 hours ago"
        });
        RecentActivities.Add(new ActivityInfo
        {
            Icon = "📤",
            Description = "Shared Driver's License",
            TimeAgo = "1 day ago"
        });
        
        HasRecentActivity = RecentActivities.Count > 0;
    }

    private static string GetIconForType(string type) => type?.ToLower() switch
    {
        "nationalid" => "🆔",
        "driverslicense" => "🚗",
        "diploma" => "🎓",
        "passport" => "✈️",
        _ => "📄"
    };

    private static string GetExpiryText(DateTime? expiryDate)
    {
        if (!expiryDate.HasValue) return "No expiration";
        
        var daysUntilExpiry = (expiryDate.Value - DateTime.Now).Days;
        if (daysUntilExpiry < 0) return $"Expired: {expiryDate.Value:MM/dd/yyyy}";
        if (daysUntilExpiry < 30) return $"Expires soon: {expiryDate.Value:MM/dd/yyyy}";
        return $"Expires: {expiryDate.Value:MM/yyyy}";
    }

    private static Color GetStatusColor(string status) => status?.ToLower() switch
    {
        "active" => Colors.Green,
        "expiring" => Colors.Orange,
        "expired" => Colors.Red,
        "revoked" => Colors.Gray,
        _ => Colors.Gray
    };

    [RelayCommand]
    private async Task AddCredential()
    {
        await _navigationService.NavigateToAsync("addcredential");
    }

    [RelayCommand]
    private async Task ShareCredential()
    {
        if (Credentials.Any())
        {
            await _navigationService.NavigateToAsync("selectivedisclosure");
        }
        // Note: Alert dialogs should be handled by the view layer
    }

    [RelayCommand]
    private async Task ScanQR()
    {
        await _navigationService.NavigateToAsync("scanner", new Dictionary<string, object>
        {
            ["mode"] = ScanMode.CredentialOffer.ToString()
        });
    }

    [RelayCommand]
    private async Task ViewActivity()
    {
        await _navigationService.NavigateToAsync("activity");
    }

    [RelayCommand]
    private async Task ViewCredential(CredentialCardInfo credential)
    {
        await _navigationService.NavigateToAsync("credentialdetails", new Dictionary<string, object>
        {
            ["credentialId"] = credential.Id
        });
    }

    [RelayCommand]
    private async Task NavigateToBackup()
    {
        await _navigationService.NavigateToAsync("backup");
    }

    [RelayCommand]
    private async Task NavigateToSettings()
    {
        await _navigationService.NavigateToAsync("settings");
    }
}

public class CredentialCardInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "📄";
    public string ExpiryInfo { get; set; } = string.Empty;
    public Color StatusColor { get; set; } = Colors.Gray;
}

public class ActivityInfo
{
    public string Icon { get; set; } = "📝";
    public string Description { get; set; } = string.Empty;
    public string TimeAgo { get; set; } = string.Empty;
}
