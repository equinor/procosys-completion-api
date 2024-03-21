using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Caches;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.Persons;

public class PersonCache: IPersonCache
{
    private readonly ICacheManager _cacheManager;
    private readonly IPersonApiService _personApiService;
    private readonly IOptionsMonitor<CacheOptions> _options;

    public PersonCache(
        ICacheManager cacheManager,
        IPersonApiService personApiService,
        IOptionsMonitor<CacheOptions> options)
    {
        _cacheManager = cacheManager;
        _personApiService = personApiService;
        _options = options;
    }

    public async Task<List<ProCoSys4Person>> GetAllPersonsAsync(string plant, CancellationToken cancellationToken)
        => await _cacheManager.GetOrCreate(
            PersonsCacheKey(plant),
            async () =>
            {
                var persons = await _personApiService.GetAllPersonsAsync(plant, cancellationToken);
                return persons;
            },
            CacheDuration.Minutes,
            _options.CurrentValue.PersonCacheMinutes);

    private string PersonsCacheKey(string plant)
        => $"PERSONS_{plant.ToUpper()}";
}

