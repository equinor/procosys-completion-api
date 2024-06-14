using System.Net.Http.Headers;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;

public class SyncBearerTokenHandler : DelegatingHandler
{
    private readonly ISyncTokenService _syncTokenService;

    public SyncBearerTokenHandler(ISyncTokenService syncTokenService)
    {
        _syncTokenService = syncTokenService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _syncTokenService.AquireTokenAsync();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}   
