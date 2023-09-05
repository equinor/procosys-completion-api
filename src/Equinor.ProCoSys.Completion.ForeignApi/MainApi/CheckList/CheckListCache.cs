using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Caches;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
public class CheckListCache : ICheckListCache
{
    private readonly ICacheManager _cacheManager;
    private readonly ICheckListApiService _checkListApiService;
    private readonly int _checkListCacheInMinutes = 20;

    public CheckListCache(ICacheManager cacheManager, ICheckListApiService checkListApiService)
    {
        _cacheManager = cacheManager;
        _checkListApiService = checkListApiService;
    }

    public async Task<ProCoSys4CheckList?> GetCheckListAsync(string plant, Guid checkListGuid)
        => await _cacheManager.GetOrCreate(
            CheckListGuidCacheKey(checkListGuid),
            async () =>
            {
                var checkList = await _checkListApiService.GetCheckListAsync(plant, checkListGuid);
                return checkList;
            },
            CacheDuration.Minutes,
            _checkListCacheInMinutes);

    private string CheckListGuidCacheKey(Guid checkListGuid)
        => $"CHECKLIST_{checkListGuid.ToString().ToUpper()}";
}
