using CredVault.Mobile.Services;
using CredVault.Mobile.Tests.Fakes;
using CredVault.Mobile.ViewModels;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CredVault.Mobile.Tests.ViewModels;

public sealed class QrScannerViewModelTests
{
    [Fact]
    public void SetScanMode_UpdatesInstructionForVerifierScan()
    {
        var viewModel = new QrScannerViewModel(new TestNavigationService(), NullLogger<QrScannerViewModel>.Instance);

        viewModel.SetScanMode(ScanMode.VerifierScan);

        Assert.Equal(ScanMode.VerifierScan, viewModel.CurrentScanMode);
        Assert.Equal("Scan holder's credential QR code", viewModel.ScanInstruction);
    }
}
