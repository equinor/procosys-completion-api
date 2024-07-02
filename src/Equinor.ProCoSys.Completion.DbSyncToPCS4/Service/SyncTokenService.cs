using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;

public class SyncTokenService(IOptionsMonitor<SyncToPCS4Options> options, ITokenAcquisition tokenAcquisition)
    : ISyncTokenService
{
    public async Task<string> AquireTokenAsync()
    {
        try
        {
            var appToken = await tokenAcquisition.GetAccessTokenForAppAsync(options.CurrentValue.Scope);
            return appToken;
        }
        catch (Exception ex) {
            throw new Exception("There was a problem fetching the PCS4 Database Sync Bearer Token", ex);
        }
    }
}
