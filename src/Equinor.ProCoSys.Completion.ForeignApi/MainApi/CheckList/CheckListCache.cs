using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Caches;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
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

    public async Task<IReadOnlyCollection<TagCheckList>> GetCheckListsByTagIdAsync(int tagId, string plant, CancellationToken cancellationToken)
    {
        var checkListGuidCacheKey = CheckListBygTagAndPlantCacheKey(tagId, plant);

        var cachedChecklist = await distributedCache.GetStringAsync(checkListGuidCacheKey, cancellationToken);
        if (string.IsNullOrEmpty(cachedChecklist))
        {
            var checkList = await checkListApiService.GetCheckListsByTagIdAndPlantAsync(tagId, plant, cancellationToken);
            await distributedCache.SetStringAsync(checkListGuidCacheKey, JsonSerializer.Serialize(checkList), _options,
                cancellationToken);
            return checkList;
        }

        try
        {
            var checkLists = JsonSerializer.Deserialize<TagCheckList[]>(cachedChecklist) ?? [];
            return checkLists
                .Select(x => x with { Plant = plant })
                .ToArray();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to Deserialize CheckList {CacheKey}", checkListGuidCacheKey);
        }

        return [];
    }

    private static string CheckListGuidCacheKey(Guid checkListGuid)
        => $"CHECKLIST_{checkListGuid.ToString().ToUpper()}";

    private static string CheckListBygTagAndPlantCacheKey(int tagId, string plant)
        => $"CHECKLIST_tagId_{tagId}_plant_{plant}";
}
