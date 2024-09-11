using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Caches;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.ForeignApiTests.MainApi.CheckList;

[TestClass]
public class CheckListCacheTests
{
    private CheckListCache _dutWithRealCache;
    private CheckListCache _dutWithMockedCache;
    private ProCoSys4CheckList _checkList1;
    private ProCoSys4CheckList _checkList2;
    private readonly Guid _checkListGuid1 = Guid.NewGuid();
    private readonly Guid _checkListGuid2 = Guid.NewGuid();
    private ICheckListApiService _checkListApiServiceMock;
    private ICacheManager _cacheManagerMock;
    private IOptionsMonitor<ApplicationOptions> _applicationOptionsMock;
    private readonly string _plant = "P";
    private readonly string _tagNo = "T";
    private readonly string _respCode = "RC";
    private readonly string _formType = "FT";

    [TestInitialize]
    public void Setup()
    {
        _applicationOptionsMock = Substitute.For<IOptionsMonitor<ApplicationOptions>>();
        _applicationOptionsMock.CurrentValue.Returns(new ApplicationOptions { CheckListCacheExpirationMinutes = 1 });
        _checkListApiServiceMock = Substitute.For<ICheckListApiService>();
        _checkList1 = new ProCoSys4CheckList(
            _checkListGuid1,
            "FT1",
            "FG1",
            "RC1",
            "TRC1",
            "TRD1",
            "TFC1",
            "TFD1",
            false, 
            Guid.NewGuid());
        _checkList2 = new ProCoSys4CheckList(
            _checkListGuid2,
            "FT2",
            "FG2",
            "RC2",
            "TRC2",
            "TRD2",
            "TFC2",
            "TFD2",
            false, 
            Guid.NewGuid());
        _checkListApiServiceMock.GetCheckListAsync(_checkListGuid1, Arg.Any<CancellationToken>()).Returns(_checkList1);
        _checkListApiServiceMock.GetCheckListGuidByMetaInfoAsync(_plant, _tagNo, _respCode, _formType, Arg.Any<CancellationToken>())
            .Returns(_checkListGuid1);
        _cacheManagerMock = Substitute.For<ICacheManager>();

        var options = new OptionsWrapper<MemoryDistributedCacheOptions>(
            new MemoryDistributedCacheOptions()
        );
        _dutWithRealCache = new CheckListCache(
            new DistributedCacheManager(new MemoryDistributedCache(options), Substitute.For<ILogger<DistributedCacheManager>>()),
            _checkListApiServiceMock,
            _applicationOptionsMock);
        _dutWithMockedCache = new CheckListCache(_cacheManagerMock, _checkListApiServiceMock, _applicationOptionsMock);
    }

    [TestMethod]
    public async Task GetCheckList_ShouldReturnCheckListFromCheckListApiServiceFirstTime()
    {   
        // Act
        var result = await _dutWithRealCache.GetCheckListAsync(_checkListGuid1, default);

        // Assert
        AssertCheckList(_checkList1, result);
        await _checkListApiServiceMock.Received(1).GetCheckListAsync(_checkListGuid1, Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task GetCheckList_ShouldReturnCheckListsFromCacheSecondTime()
    {
        // Arrange
        await _dutWithRealCache.GetCheckListAsync(_checkListGuid1, default);

        // Act
        var result = await _dutWithRealCache.GetCheckListAsync(_checkListGuid1, default);

        // Assert
        AssertCheckList(_checkList1, result);
        // since GetCheckListAsync has been called twice, but GetCheckListAsync has been called once, the second Get uses cache
        await _checkListApiServiceMock.Received(1).GetCheckListAsync(_checkListGuid1, Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task GetCheckListGuidByMetaInfo_ShouldReturnCheckListFromCheckListApiServiceFirstTime()
    {
        // Act
        var result = await _dutWithRealCache.GetCheckListGuidByMetaInfoAsync(_plant, _tagNo, _respCode, _formType, default);

        // Assert
        Assert.AreEqual(_checkListGuid1, result);
        await _checkListApiServiceMock.Received(1).GetCheckListGuidByMetaInfoAsync(_plant, _tagNo, _respCode, _formType, Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task GetCheckListGuidByMetaInfo_ShouldReturnCheckListsFromCacheSecondTime()
    {
        // Arrange
        await _dutWithRealCache.GetCheckListGuidByMetaInfoAsync(_plant, _tagNo, _respCode, _formType, default);

        // Act
        var result = await _dutWithRealCache.GetCheckListGuidByMetaInfoAsync(_plant, _tagNo, _respCode, _formType, default);

        // Assert
        Assert.AreEqual(_checkListGuid1, result);
        // since GetCheckListGuidByMetaInfoAsync has been called twice, but GetCheckListGuidByMetaInfoAsync has been called once, the second Get uses cache
        await _checkListApiServiceMock.Received(1).GetCheckListGuidByMetaInfoAsync(_plant, _tagNo, _respCode, _formType, Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task GetManyCheckListsAsync_ShouldReturnCheckListsFromCheckListApiService_WhenNotFoundInCache()
    {
        // Arrange
        _cacheManagerMock.GetManyAsync<ProCoSys4CheckList>(Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _checkListApiServiceMock.GetManyCheckListsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([_checkList1, _checkList2]);

        // Act
        var result = await _dutWithMockedCache.GetManyCheckListsAsync([_checkListGuid1, _checkListGuid2], default);

        // Assert
        AssertCheckList(_checkList1, result.Single(c => c.CheckListGuid == _checkListGuid1));
        AssertCheckList(_checkList2, result.Single(c => c.CheckListGuid == _checkListGuid2));
    }

    [TestMethod]
    public async Task GetManyCheckListsAsync_ShouldReturnCheckListsFromCache()
    {
        // Arrange
        _cacheManagerMock.GetManyAsync<ProCoSys4CheckList>(Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns([_checkList1, _checkList2]);
        _checkListApiServiceMock.GetManyCheckListsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var result = await _dutWithMockedCache.GetManyCheckListsAsync([_checkListGuid1, _checkListGuid2], default);

        // Assert
        AssertCheckList(_checkList1, result.Single(c => c.CheckListGuid == _checkListGuid1));
        AssertCheckList(_checkList2, result.Single(c => c.CheckListGuid == _checkListGuid2));
    }

    [TestMethod]
    public async Task GetManyCheckListsAsync_ShouldAddCheckListsFromCheckListApiServiceToCache_WhenNotFoundInCache()
    {
        // Arrange
        _cacheManagerMock.GetManyAsync<ProCoSys4CheckList>(Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _checkListApiServiceMock.GetManyCheckListsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([_checkList1, _checkList2]);

        // Act
        await _dutWithMockedCache.GetManyCheckListsAsync([_checkListGuid1, _checkListGuid2], default);

        // Assert
        await _cacheManagerMock.Received(1).CreateAsync(
            CheckListCache.CheckListGuidCacheKey(_checkListGuid1), _checkList1, CacheDuration.Minutes,
            _applicationOptionsMock.CurrentValue.CheckListCacheExpirationMinutes, Arg.Any<CancellationToken>());
        await _cacheManagerMock.Received(1).CreateAsync(
            CheckListCache.CheckListGuidCacheKey(_checkListGuid2), _checkList2, CacheDuration.Minutes,
            _applicationOptionsMock.CurrentValue.CheckListCacheExpirationMinutes, Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task GetManyCheckListsAsync_ShouldNotAddCheckListsToCache_WhenAlreadyFoundInCache()
    {
        // Arrange
        _cacheManagerMock.GetManyAsync<ProCoSys4CheckList>(Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns([_checkList1, _checkList2]);
        _checkListApiServiceMock.GetManyCheckListsAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await _dutWithMockedCache.GetManyCheckListsAsync([_checkListGuid1, _checkListGuid2], default);

        // Assert
        await _cacheManagerMock.Received(0).CreateAsync(
            Arg.Any<string>(),
            Arg.Any<ProCoSys4CheckList>(),
            Arg.Any<CacheDuration>(),
            Arg.Any<long>(),
            Arg.Any<CancellationToken>());
    }

    private void AssertCheckList(ProCoSys4CheckList expected, ProCoSys4CheckList actual)
    {
        Assert.AreEqual(expected.IsVoided, actual.IsVoided);
        Assert.AreEqual(expected.ProjectGuid, actual.ProjectGuid);
        Assert.AreEqual(expected.ResponsibleCode, actual.ResponsibleCode);
    }
}
