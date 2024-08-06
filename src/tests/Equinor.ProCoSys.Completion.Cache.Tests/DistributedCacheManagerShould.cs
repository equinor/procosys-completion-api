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
    public void GetOrCreate_ShouldCacheEntry()
    {
        var dut = new DistributedCacheManager(new MemoryDistributedCache(_options));
        var key = "test";
        var item = "foo";

        dut.GetOrCreate(key, () => item, CacheDuration.Minutes, 1);
        var results = dut.Get<string>(key);

        Assert.AreEqual(item, results);
    }

    [TestMethod]
    public void GetOrCreate_ShouldReturnNull_WhenKeyNotFound()
    {
        var dut = new DistributedCacheManager(new MemoryDistributedCache(_options));
        var key = "test";
        var item = "foo";

        dut.GetOrCreate(key, () => item, CacheDuration.Minutes, 1);
        var results = dut.Get<string>("wrong key");

        Assert.IsNull(results);
    }

    [TestMethod]
    public void RemoveEntry_ShouldRemoveEntry()
    {
        var dut = new DistributedCacheManager(new MemoryDistributedCache(_options));
        var key = "test";
        var item = "foo";

        dut.GetOrCreate(key, () => item, CacheDuration.Minutes, 1);
        var results = dut.Get<string>(key);
        dut.Remove(key);
        var removedResults = dut.Get<string>(key);

        Assert.AreEqual(item, results);
        Assert.IsNull(removedResults);
    }

    [TestMethod]
    public void GetOrCreate_ShouldCacheEntry_WhenActionIsTask()
    {
        var dut = new DistributedCacheManager(new MemoryDistributedCache(_options));
        var key = "test";
        var item = "foo";

        dut.GetOrCreate(key, async () =>
        {
            await Task.Delay(1);
            return item;
        }, CacheDuration.Minutes, 1);
        var results = dut.Get<Task<string>>(key);

        Assert.AreEqual(item, results.Result);
    }
}
