using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

public class CheckListCache(
    ICheckListApiService checkListApiService,
    IDistributedCache distributedCache,
    IOptionsMonitor<ApplicationOptions> applicationOptions,
    ILogger<CheckListCache> logger)
    : ICheckListCache
{
    private readonly DistributedCacheEntryOptions _options = new()
    {
        SlidingExpiration = TimeSpan.FromMinutes(applicationOptions.CurrentValue.CheckListCacheExpirationMinutes)
    };

    public async Task<ProCoSys4CheckList?> GetCheckListAsync(Guid checkListGuid,
        CancellationToken cancellationToken)
    {
        var checkListGuidCacheKey = CheckListGuidCacheKey(checkListGuid);

        var cachedChecklist = await distributedCache.GetStringAsync(checkListGuidCacheKey, cancellationToken);
        if (string.IsNullOrEmpty(cachedChecklist))
        {
            var checkList = await checkListApiService.GetCheckListAsync(checkListGuid, cancellationToken);
            await distributedCache.SetStringAsync(checkListGuidCacheKey, JsonSerializer.Serialize(checkList), _options,
                cancellationToken);
            return checkList;
        }

        try
        {
            return JsonSerializer.Deserialize<ProCoSys4CheckList?>(cachedChecklist);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to Deserialize CheckList {CacheKey}", checkListGuidCacheKey);
        }

        return null;
    }

    private static string CheckListGuidCacheKey(Guid checkListGuid)
        => $"CHECKLIST_{checkListGuid.ToString().ToUpper()}";
}
