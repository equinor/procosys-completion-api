using System;
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
    private CheckListCache _dut;
    private ProCoSys4CheckList _checkList;
    private readonly Guid _checkListGuid = new("{3BFB54C7-91E2-422E-833F-951AD07FE37F}");
    private ICheckListApiService _checkListApiServiceMock;

    [TestInitialize]
    public void Setup()
    {
        var applicationOptionsMock = Substitute.For<IOptionsMonitor<ApplicationOptions>>();
        applicationOptionsMock.CurrentValue.Returns(new ApplicationOptions { CheckListCacheExpirationMinutes = 1 });
        _checkListApiServiceMock = Substitute.For<ICheckListApiService>();
        _checkList = new ProCoSys4CheckList("RX", false, Guid.NewGuid());
        _checkListApiServiceMock.GetCheckListAsync(_checkListGuid, Arg.Any<CancellationToken>()).Returns(_checkList);

        var options = new OptionsWrapper<MemoryDistributedCacheOptions>(
            new MemoryDistributedCacheOptions()
        );
        _dut = new CheckListCache(
            _checkListApiServiceMock, 
            new DistributedCacheManager(new MemoryDistributedCache(options), Substitute.For<ILogger<DistributedCacheManager>>()), 
            applicationOptionsMock);
    }

    [TestMethod]
    public async Task GetCheckList_ShouldReturnCheckListFromCheckListApiServiceFirstTime()
    {   
        // Act
        var result = await _dut.GetCheckListAsync(_checkListGuid, default);

        // Assert
        AssertCheckList(result);
        await _checkListApiServiceMock.Received(1).GetCheckListAsync(_checkListGuid, Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task GetCheckList_ShouldReturnCheckListsFromCacheSecondTime()
    {
        await _dut.GetCheckListAsync(_checkListGuid, default);

        // Act
        var result = await _dut.GetCheckListAsync(_checkListGuid, default);

        // Assert
        AssertCheckList(result);
        // since GetCheckListAsync has been called twice, but TryGetCheckListByOidAsync has been called once, the second Get uses cache
        await _checkListApiServiceMock.Received(1).GetCheckListAsync(_checkListGuid, Arg.Any<CancellationToken>());
    }

    private void AssertCheckList(ProCoSys4CheckList checkList)
    {
        Assert.AreEqual(_checkList.IsVoided, checkList.IsVoided);
        Assert.AreEqual(_checkList.ProjectGuid, checkList.ProjectGuid);
        Assert.AreEqual(_checkList.ResponsibleCode, checkList.ResponsibleCode);
    }
}
