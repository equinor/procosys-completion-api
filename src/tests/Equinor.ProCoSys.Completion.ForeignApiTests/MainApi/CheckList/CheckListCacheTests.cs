using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

namespace Equinor.ProCoSys.Completion.ForeignApiTests.MainApi.CheckList;

[TestClass]
public class CheckListCacheTests
{
    private CheckListCache _dut;
    private ProCoSys4CheckList _checkList;
    private readonly Guid _checkListGuid = new("{3BFB54C7-91E2-422E-833F-951AD07FE37F}");
    private ICheckListApiService _checkListApiServiceMock;
    private IDistributedCache _distributedCache;

    [TestInitialize]
    public void Setup()
    {
        TimeService.SetProvider(new ManualTimeProvider(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        var options = new OptionsWrapper<MemoryDistributedCacheOptions>(
            new MemoryDistributedCacheOptions()
        );
        
        var applicationOptionsMock = Substitute.For<IOptionsMonitor<ApplicationOptions>>();
        applicationOptionsMock.CurrentValue.Returns(new ApplicationOptions { CheckListCacheExpirationMinutes = 1 });
        _checkListApiServiceMock = Substitute.For<ICheckListApiService>();
        _distributedCache = new MemoryDistributedCache(options);
        _checkList = new ProCoSys4CheckList("RX", false, Guid.NewGuid());
        _checkListApiServiceMock.GetCheckListAsync(_checkListGuid, default).Returns(_checkList);

        _dut = new CheckListCache(_checkListApiServiceMock, _distributedCache, applicationOptionsMock, default);
    }

    [TestMethod]
    public async Task GetCheckList_ShouldReturnCheckListFromCheckListApiServiceFirstTime()
    {   
        // Act
        var result = await _dut.GetCheckListAsync(_checkListGuid, default);

        // Assert
        AssertCheckList(result);
        await _checkListApiServiceMock.Received(1).GetCheckListAsync(_checkListGuid, default);
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
        await _checkListApiServiceMock.Received(1).GetCheckListAsync(_checkListGuid, default);
    }

    private void AssertCheckList(ProCoSys4CheckList checkList)
    {
        Assert.AreEqual(_checkList.IsVoided, checkList.IsVoided);
        Assert.AreEqual(_checkList.ProjectGuid, checkList.ProjectGuid);
        Assert.AreEqual(_checkList.ResponsibleCode, checkList.ResponsibleCode);
    }
}
