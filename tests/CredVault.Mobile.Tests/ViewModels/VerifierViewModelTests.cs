using System.Threading.Tasks;
using CredVault.Mobile.Services;
using CredVault.Mobile.Tests.Fakes;
using CredVault.Mobile.ViewModels;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CredVault.Mobile.Tests.ViewModels;

public sealed class VerifierViewModelTests
{
    private static WalletService CreateWalletService()
    {
        return new WalletService(new StubWalletApiClient(), NullLogger<WalletService>.Instance);
    }

    [Fact]
    public async Task ScanCredentialCommand_NavigatesToScannerInVerifierMode()
    {
        var navigation = new TestNavigationService();
        var viewModel = new VerifierViewModel(navigation, CreateWalletService(), NullLogger<VerifierViewModel>.Instance);

        await viewModel.ScanCredentialQrCommand.ExecuteAsync(null);

        Assert.Equal("scanner", Assert.Single(navigation.Routes));
        var parameters = Assert.Single(navigation.Parameters);
        Assert.True(parameters.TryGetValue("mode", out var modeValue));
        Assert.Equal(ScanMode.VerifierScan.ToString(), modeValue);
    }

    [Fact]
    public async Task ManualRequestCommand_NavigatesToManualRequest()
    {
        var navigation = new TestNavigationService();
        var viewModel = new VerifierViewModel(navigation, CreateWalletService(), NullLogger<VerifierViewModel>.Instance);

        await viewModel.CreateManualRequestCommand.ExecuteAsync(null);

        Assert.Equal("manualrequest", Assert.Single(navigation.Routes));
    }
}
