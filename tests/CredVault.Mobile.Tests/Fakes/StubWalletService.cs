using CredVault.Mobile.Core.Services;
using CredVault.Mobile.Core.Models;

namespace CredVault.Mobile.Tests.Fakes;

public class StubWalletService : IWalletService
{
    public Task<ServiceResult<List<CredentialInfo>>> GetCredentialsAsync()
    {
        return Task.FromResult(new ServiceResult<List<CredentialInfo>>
        {
            IsSuccess = true,
            Data = new List<CredentialInfo>()
        });
    }

    public Task<ServiceResult<CredentialInfo>> GetCredentialAsync(string credentialId)
    {
        return Task.FromResult(new ServiceResult<CredentialInfo>
        {
            IsSuccess = true,
            Data = new CredentialInfo { Id = credentialId }
        });
    }
}
