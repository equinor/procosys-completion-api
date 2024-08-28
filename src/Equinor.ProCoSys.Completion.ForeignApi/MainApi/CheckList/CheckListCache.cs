using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Caches;
using Equinor.ProCoSys.Completion.Domain;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

public class CheckListCache(
    ICacheManager cacheManager,
    ICheckListApiService checkListApiService,
    IOptionsMonitor<ApplicationOptions> applicationOptions)
    : ICheckListCache
{

    public async Task<ProCoSys4CheckList?> GetCheckListAsync(Guid checkListGuid, CancellationToken cancellationToken)
        => await cacheManager.GetOrCreateAsync(
            CheckListGuidCacheKey(checkListGuid),
            token => checkListApiService.GetCheckListAsync(checkListGuid, token),
            CacheDuration.Minutes,
            applicationOptions.CurrentValue.CheckListCacheExpirationMinutes,
            cancellationToken);

    private static string CheckListGuidCacheKey(Guid checkListGuid)
        => $"CHKLIST_{checkListGuid.ToString().ToUpper()}";
}
