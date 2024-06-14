using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Equinor.ProCoSys.Auth.Authentication;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;

public class SyncTokenService : ISyncTokenService
{
    private readonly IConfidentialClientApplication _app;
    private readonly IAuthenticatorOptions _authenticatorOptions;
    private readonly IOptionsMonitor<SyncToPCS4Options> _options;

    public SyncTokenService(IAuthenticatorOptions authenticatorOptions, IOptionsMonitor<SyncToPCS4Options> options)
    {
        _authenticatorOptions = authenticatorOptions;
        _options = options;
        _app = ConfidentialClientApplicationBuilder.Create(_authenticatorOptions.ClientId)
            .WithClientSecret(_authenticatorOptions.ClientSecret)
            .WithAuthority(new Uri($"{_authenticatorOptions.Authority}"))
            .Build();
    }

    public async Task<string> AquireTokenAsync()
    {
        try
        {
            var optionValues = _options.CurrentValue;
            var result = await _app.AcquireTokenForClient([optionValues.Scope]).ExecuteAsync();

            return result.AccessToken;
        }
        catch (Exception ex) {
            throw new Exception("There was a problem fetching the PCS4 Database Sync Bearer Token", ex);
        }
    }
}
