using System.Net.Http.Headers;
using Equinor.ProCoSys.Common.Caches;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;

public class SyncBearerTokenHandler : DelegatingHandler
{
    private readonly ISyncTokenService _syncTokenService;
    private readonly ICacheManager _cacheManager;
    private readonly int _syncTokenCacheInMinutes = 60;

    public SyncBearerTokenHandler(ISyncTokenService syncTokenService, ICacheManager cacheManager)
    {
        _syncTokenService = syncTokenService;
        _cacheManager = cacheManager;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _cacheManager.GetOrCreate(
            "SYNC_TOKEN",
            async () =>
            {
                var checkList = await _syncTokenService.AquireTokenAsync();
                return checkList;
            },
            CacheDuration.Minutes,
            _syncTokenCacheInMinutes);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}   
