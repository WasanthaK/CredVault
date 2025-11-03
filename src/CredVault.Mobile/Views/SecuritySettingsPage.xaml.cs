using CredVault.Mobile.Helpers;
using CredVault.Mobile.ViewModels;

namespace CredVault.Mobile.Views;

public partial class SecuritySettingsPage : ContentPage
{
    public SecuritySettingsPage()
        : this(ServiceHelper.GetRequiredService<SecuritySettingsViewModel>())
    {
    }

    public SecuritySettingsPage(SecuritySettingsViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        InitializeComponent();
        BindingContext = viewModel;
    }
}
