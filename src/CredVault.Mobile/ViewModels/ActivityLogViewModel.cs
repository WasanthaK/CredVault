using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CredVault.Mobile.Models;
using CredVault.Mobile.Services;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace CredVault.Mobile.ViewModels;

public partial class ActivityLogViewModel : ObservableObject
{
    private readonly WalletService _walletService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<ActivityLogViewModel> _logger;

    [ObservableProperty]
    private string title = "Activity Log";

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private bool hasMoreItems = true;

    [ObservableProperty]
    private string selectedFilter = "All";

    private int _currentPage = 1;
    private const int PageSize = 20;

    public ObservableCollection<ActivityLogItem> Activities { get; } = new();

    public List<string> FilterOptions { get; } = new()
    {
        "All",
        "Issued",
        "Shared",
        "Revoked",
        "Suspended"
    };

    public ActivityLogViewModel(
        WalletService walletService,
        INavigationService navigationService,
        ILogger<ActivityLogViewModel> logger)
    {
        _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InitializeAsync()
    {
        await LoadActivitiesAsync();
    }

    [RelayCommand]
    private async Task LoadActivitiesAsync()
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;
            _currentPage = 1;
            Activities.Clear();

            _logger.LogInformation("Loading activity logs");

            var result = await _walletService.GetActivityLogsAsync(_currentPage, PageSize);

            if (result.IsSuccess && result.Data != null)
            {
                foreach (var log in result.Data)
                {
                    Activities.Add(MapToActivityLogItem(log));
                }

                HasMoreItems = result.Data.Count == PageSize;
                _logger.LogInformation("Loaded {Count} activity logs", result.Data.Count);
            }
            else
            {
                _logger.LogWarning("Failed to load activity logs: {Message}", result.ErrorMessage);
                if (Application.Current?.Windows.Count > 0)
                {
                    await Application.Current.Windows[0].Page!.DisplayAlert(
                        "Error",
                        result.ErrorMessage ?? "Failed to load activity logs",
                        "OK");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading activity logs");
            if (Application.Current?.Windows.Count > 0)
            {
                await Application.Current.Windows[0].Page!.DisplayAlert(
                    "Error",
                    "An unexpected error occurred while loading activity logs",
                    "OK");
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshActivitiesAsync()
    {
        if (IsRefreshing)
            return;

        try
        {
            IsRefreshing = true;
            _currentPage = 1;
            Activities.Clear();

            _logger.LogInformation("Refreshing activity logs");

            var result = await _walletService.GetActivityLogsAsync(_currentPage, PageSize);

            if (result.IsSuccess && result.Data != null)
            {
                foreach (var log in result.Data)
                {
                    Activities.Add(MapToActivityLogItem(log));
                }

                HasMoreItems = result.Data.Count == PageSize;
                _logger.LogInformation("Refreshed {Count} activity logs", result.Data.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing activity logs");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task LoadMoreActivitiesAsync()
    {
        if (IsLoading || !HasMoreItems)
            return;

        try
        {
            IsLoading = true;
            _currentPage++;

            _logger.LogInformation("Loading more activity logs, page {Page}", _currentPage);

            var result = await _walletService.GetActivityLogsAsync(_currentPage, PageSize);

            if (result.IsSuccess && result.Data != null)
            {
                foreach (var log in result.Data)
                {
                    Activities.Add(MapToActivityLogItem(log));
                }

                HasMoreItems = result.Data.Count == PageSize;
                _logger.LogInformation("Loaded {Count} more activity logs", result.Data.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading more activity logs");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task FilterChangedAsync()
    {
        _logger.LogInformation("Filter changed to: {Filter}", SelectedFilter);
        
        // In a production app, you would filter server-side or client-side
        // For now, just reload
        await LoadActivitiesAsync();
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await _navigationService.GoBackAsync();
    }

    private ActivityLogItem MapToActivityLogItem(TransactionLogResponseDto log)
    {
        var action = log.Action ?? "Unknown";
        var icon = GetIconForAction(action);
        var color = GetColorForAction(action);
        var description = log.Details ?? $"{action} action performed";

        return new ActivityLogItem
        {
            Id = log.Id ?? Guid.NewGuid().ToString(),
            Timestamp = log.Timestamp,
            Action = action,
            Description = description,
            CredentialId = log.CredentialId,
            Icon = icon,
            IconColor = color,
            TimeAgo = GetTimeAgo(log.Timestamp)
        };
    }

    private string GetIconForAction(string action)
    {
        return action.ToLower() switch
        {
            "issued" or "create" => "&#xE89C;", // add_circle
            "shared" or "present" => "&#xE80D;", // share
            "revoked" => "&#xE14C;", // cancel
            "suspended" => "&#xE8F5;", // pause_circle
            "reactivated" => "&#xE86C;", // check_circle
            "exported" => "&#xF8A1;", // upload
            "imported" => "&#xF090;", // download
            _ => "&#xE889;" // info
        };
    }

    private string GetColorForAction(string action)
    {
        return action.ToLower() switch
        {
            "issued" or "create" or "reactivated" => "#4caf50", // green
            "shared" or "present" => "#2196f3", // blue
            "revoked" => "#f44336", // red
            "suspended" => "#ff9800", // orange
            "exported" or "imported" => "#9c27b0", // purple
            _ => "#757575" // gray
        };
    }

    private string GetTimeAgo(DateTime timestamp)
    {
        var timeSpan = DateTime.Now - timestamp;

        if (timeSpan.TotalMinutes < 1)
            return "Just now";
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes}m ago";
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours}h ago";
        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays}d ago";
        if (timeSpan.TotalDays < 30)
            return $"{(int)(timeSpan.TotalDays / 7)}w ago";
        
        return timestamp.ToString("MMM dd, yyyy");
    }
}

public class ActivityLogItem : ObservableObject
{
    public required string Id { get; set; }
    public DateTime Timestamp { get; set; }
    public required string Action { get; set; }
    public required string Description { get; set; }
    public string? CredentialId { get; set; }
    public required string Icon { get; set; }
    public required string IconColor { get; set; }
    public required string TimeAgo { get; set; }
}
