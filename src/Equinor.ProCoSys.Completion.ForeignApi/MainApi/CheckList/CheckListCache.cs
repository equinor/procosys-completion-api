using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

public class CheckListCache(
    ICheckListApiService checkListApiService,
    IDistributedCache distributedCache,
    ILogger<CheckListCache> logger)
    : ICheckListCache
{
    private readonly DistributedCacheEntryOptions _options = new() { SlidingExpiration = TimeSpan.FromMinutes(20) };

    public async Task<ProCoSys4CheckList?> GetCheckListAsync(Guid checkListGuid)
    {
        var checkListGuidCacheKey = CheckListGuidCacheKey(checkListGuid);

        var cachedChecklist = await distributedCache.GetStringAsync(checkListGuidCacheKey);
        if (string.IsNullOrEmpty(cachedChecklist))
        {
            var checkList = await checkListApiService.GetCheckListAsync(checkListGuid);
            await distributedCache.SetStringAsync(checkListGuidCacheKey, JsonSerializer.Serialize(checkList), _options);
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
