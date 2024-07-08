using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Caches;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Cache.Tests;

[TestClass]
public class DistributedCacheManagerShould
{
    private readonly OptionsWrapper<MemoryDistributedCacheOptions> _options = new(
        new MemoryDistributedCacheOptions()
    );

    [TestMethod]
    public void CacheEntry()
    {
        var manager = new DistributedCacheManager(new MemoryDistributedCache(_options));
        var key = "test";
        var item = "foo";

        manager.GetOrCreate(key, () => item, CacheDuration.Minutes, 1);
        var results = manager.Get<string>(key);

        Assert.AreEqual(item, results);
    }

    [TestMethod]
    public void ReturnNullOnNotFound()
    {
        var manager = new DistributedCacheManager(new MemoryDistributedCache(_options));
        var key = "test";
        var item = "foo";

        manager.GetOrCreate(key, () => item, CacheDuration.Minutes, 1);
        var results = manager.Get<string>("wrong key");

        Assert.IsNull(results);
    }

    [TestMethod]
    public void RemoveEntry()
    {
        var manager = new DistributedCacheManager(new MemoryDistributedCache(_options));
        var key = "test";
        var item = "foo";

        manager.GetOrCreate(key, () => item, CacheDuration.Minutes, 1);
        var results = manager.Get<string>(key);
        manager.Remove(key);
        var removedResults = manager.Get<string>(key);

        Assert.AreEqual(item, results);
        Assert.IsNull(removedResults);
    }

    [TestMethod]
    public void CacheEntryTask()
    {
        var manager = new DistributedCacheManager(new MemoryDistributedCache(_options));
        var key = "test";
        var item = "foo";

        manager.GetOrCreate(key, async () =>
        {
            await Task.Delay(1);
            return item;
        }, CacheDuration.Minutes, 1);
        var results = manager.Get<Task<string>>(key);

        Assert.AreEqual(item, results.Result);
    }
}
