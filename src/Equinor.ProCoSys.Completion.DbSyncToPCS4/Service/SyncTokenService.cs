using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;

public class SyncTokenService : ISyncTokenService
{
    private readonly IConfidentialClientApplication _app;
    private readonly IOptionsMonitor<SyncToPCS4Options> _options;

    public SyncTokenService(IOptionsMonitor<SyncToPCS4Options> options)
    {
        _options = options;
        var optionValues = _options.CurrentValue;
        _app = ConfidentialClientApplicationBuilder.Create(optionValues.ClientId)
            .WithClientSecret(optionValues.ClientSecret)
            .WithAuthority(new Uri($"https://login.microsoftonline.com/{optionValues.TenantId}"))
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
