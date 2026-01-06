using System;
using System.Collections.Generic;
using System.Linq;
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

    public async Task<Guid?> GetCheckListGuidByMetaInfoAsync(
        string plant, 
        string tagNo, 
        string responsibleCode, 
        string formularType,
        string projectName,
        CancellationToken cancellationToken)
        => await cacheManager.GetOrCreateAsync(
            CheckListGuidCacheKey(plant, tagNo, responsibleCode, formularType, projectName),
            token => checkListApiService.GetCheckListGuidByMetaInfoAsync(plant, tagNo, responsibleCode, formularType, projectName, token),
            CacheDuration.Minutes,
            applicationOptions.CurrentValue.CheckListCacheExpirationMinutes,
            cancellationToken);

    public async Task<List<ProCoSys4CheckList>> GetManyCheckListsAsync(List<Guid> checkListGuids, CancellationToken cancellationToken)
    {
        var cacheKeys = new List<string>();
        foreach (var checkListGuid in checkListGuids)
        {
            cacheKeys.Add(CheckListGuidCacheKey(checkListGuid));
        }

        var checkListsFromCache = await cacheManager.GetManyAsync<ProCoSys4CheckList>(cacheKeys, cancellationToken);

        var foundCheckListGuids = checkListsFromCache.Select(c => c.CheckListGuid);
        var notFoundCheckListGuids = checkListGuids.Except(foundCheckListGuids).ToList();

        if (notFoundCheckListGuids.Count > 0)
        {
            var checkListsFromProCoSys4 =
                await checkListApiService.GetManyCheckListsAsync(notFoundCheckListGuids, cancellationToken);
            foreach (var checkList in checkListsFromProCoSys4)
            {
                await cacheManager.CreateAsync(
                    CheckListGuidCacheKey(checkList.CheckListGuid),
                    checkList,
                    CacheDuration.Minutes,
                    applicationOptions.CurrentValue.CheckListCacheExpirationMinutes,
                    cancellationToken);
            }

            checkListsFromCache.AddRange(checkListsFromProCoSys4);
        }

        return checkListsFromCache;
    }

    public static string CheckListGuidCacheKey(string plant, string tagNo, string responsibleCode, string formularType, string projectName)
        => $"CHKLIST_{plant}_{tagNo}_{responsibleCode}_{formularType}_{projectName}";

    public static string CheckListGuidCacheKey(Guid checkListGuid)
        => $"CHKLIST_{checkListGuid.ToString().ToUpper()}";
}
