using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CredVault.Mobile.Services;
using System.Collections.ObjectModel;

namespace CredVault.Mobile.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly WalletService _walletService;
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
        WalletService walletService,
        INavigationService navigationService)
    {
        _walletService = walletService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    private async Task Initialize()
    {
        try
        {
            // Set greeting based on time of day
            var hour = DateTime.Now.Hour;
            var userName = await GetUserNameAsync();
            
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
            else
            {
                // Log error but don't crash - user can still navigate
                System.Diagnostics.Debug.WriteLine($"Failed to load credentials: {result.ErrorMessage}");
            }

            // Load recent activities
            await LoadRecentActivities();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Dashboard initialization error: {ex.Message}");
            // Don't crash - let user navigate
        }
    }

    private async Task<string> GetUserNameAsync()
    {
        try
        {
            // Try to get username from SecureStorage first
            var username = await SecureStorage.GetAsync("username");
            if (!string.IsNullOrEmpty(username))
                return username;
            
            // Fallback to "User" if not found
            return "User";
        }
        catch
        {
            return "User";
        }
    }
    
    private async Task LoadRecentActivities()
    {
        try
        {
            // Load real activity logs from API
            var result = await _walletService.GetActivityLogsAsync(page: 1, pageSize: 10);
            RecentActivities.Clear();
            
            if (result.IsSuccess && result.Data != null)
            {
                foreach (var log in result.Data.Take(5)) // Show top 5 recent activities
                {
                    RecentActivities.Add(new ActivityInfo
                    {
                        Icon = GetIconForActivityType(log.Action ?? ""),
                        Description = log.Details ?? log.Action ?? "Unknown activity",
                        TimeAgo = GetTimeAgo(log.Timestamp)
                    });
                }
            }
            
            HasRecentActivity = RecentActivities.Count > 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load activity logs: {ex.Message}");
            HasRecentActivity = false;
        }
    }
    
    private static string GetIconForActivityType(string eventType) => eventType?.ToLower() switch
    {
        "issued" => "✅",
        "verified" => "✅",
        "shared" => "📤",
        "presented" => "📤",
        "revoked" => "🚫",
        "suspended" => "⏸️",
        "reactivated" => "▶️",
        _ => "📝"
    };
    
    private static string GetTimeAgo(DateTime timestamp)
    {
        var timeSpan = DateTime.Now - timestamp;
        
        if (timeSpan.TotalMinutes < 1)
            return "Just now";
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes == 1 ? "" : "s")} ago";
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours == 1 ? "" : "s")} ago";
        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays == 1 ? "" : "s")} ago";
        if (timeSpan.TotalDays < 30)
            return $"{(int)(timeSpan.TotalDays / 7)} week{((int)(timeSpan.TotalDays / 7) == 1 ? "" : "s")} ago";
        
        return timestamp.ToString("MMM d, yyyy");
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
        else
        {
            await Shell.Current.DisplayAlert("No Credentials", "Add a credential first before sharing.", "OK");
        }
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
