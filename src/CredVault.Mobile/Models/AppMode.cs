namespace CredVault.Mobile.Models;

/// <summary>
/// Represents the operational mode of the wallet application.
/// Each mode provides a different user experience tailored to specific roles.
/// </summary>
public enum AppMode
{
    /// <summary>
    /// Holder mode - Manage and present digital credentials.
    /// Primary color: Blue (#004bad)
    /// </summary>
    Holder = 0,

    /// <summary>
    /// Verifier mode - Request and verify credentials from others.
    /// Primary color: Green (#20c997)
    /// </summary>
    Verifier = 1,

    /// <summary>
    /// Issuer Assist mode - Support the issuance process of new credentials.
    /// Primary color: Orange (#e88b00)
    /// </summary>
    IssuerAssist = 2
}

/// <summary>
/// Provides metadata and UI configuration for each app mode.
/// </summary>
public class AppModeInfo
{
    public AppMode Mode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string PrimaryColor { get; set; } = string.Empty;
    public string BackgroundLight { get; set; } = string.Empty;
    public string BackgroundDark { get; set; } = string.Empty;

    public static AppModeInfo GetModeInfo(AppMode mode) => mode switch
    {
        AppMode.Holder => new AppModeInfo
        {
            Mode = AppMode.Holder,
            Name = "Holder Mode",
            Description = "Manage and present your digital credentials.",
            Icon = "account_balance_wallet",
            PrimaryColor = "#004bad",
            BackgroundLight = "#f5f7f8",
            BackgroundDark = "#0f1823"
        },
        AppMode.Verifier => new AppModeInfo
        {
            Mode = AppMode.Verifier,
            Name = "Verifier Mode",
            Description = "Request and verify credentials from others.",
            Icon = "verified_user",
            PrimaryColor = "#20c997",
            BackgroundLight = "#f6f8f7",
            BackgroundDark = "#10221b"
        },
        AppMode.IssuerAssist => new AppModeInfo
        {
            Mode = AppMode.IssuerAssist,
            Name = "Issuer Assist Mode",
            Description = "Support the issuance process of new credentials.",
            Icon = "approval_delegation",
            PrimaryColor = "#e88b00",
            BackgroundLight = "#f8f7f5",
            BackgroundDark = "#231b0f"
        },
        _ => GetModeInfo(AppMode.Holder)
    };

    public static List<AppModeInfo> GetAllModes() => new()
    {
        GetModeInfo(AppMode.Holder),
        GetModeInfo(AppMode.Verifier),
        GetModeInfo(AppMode.IssuerAssist)
    };
}
