using System.Threading.Tasks;
using CredVault.Mobile.Services;
using CredVault.Mobile.ViewModels;
using CredVault.Mobile.Tests.Fakes;
using Xunit;

namespace CredVault.Mobile.Tests.ViewModels;

public sealed class DashboardViewModelTests
{
    [Fact]
    public async Task AddCredentialCommand_NavigatesToAddCredentialRoute()
    {
        var navigation = new TestNavigationService();
        var viewModel = new DashboardViewModel(walletService: null!, navigationService: navigation);

        await viewModel.AddCredentialCommand.ExecuteAsync(null);

        Assert.Equal("addcredential", Assert.Single(navigation.Routes));
    }

    [Fact]
    public async Task ScanQrCommand_UsesCredentialOfferMode()
    {
        var navigation = new TestNavigationService();
        var viewModel = new DashboardViewModel(walletService: null!, navigationService: navigation);

        await viewModel.ScanQRCommand.ExecuteAsync(null);

        Assert.Equal("scanner", Assert.Single(navigation.Routes));
        var parameters = Assert.Single(navigation.Parameters);
        Assert.True(parameters.TryGetValue("mode", out var modeValue));
        Assert.Equal(ScanMode.CredentialOffer.ToString(), modeValue);
    }
}
