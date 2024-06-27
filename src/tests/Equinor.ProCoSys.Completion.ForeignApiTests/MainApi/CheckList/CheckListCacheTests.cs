using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.Test.Common;
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
        TimeService.SetProvider(new ManualTimeProvider(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)));

        _checkListApiServiceMock = Substitute.For<ICheckListApiService>();
        _checkList = new ProCoSys4CheckList("RX", false, Guid.NewGuid());
        _checkListApiServiceMock.GetCheckListAsync(_checkListGuid).Returns(_checkList);

        _dut = new CheckListCache(_checkListApiServiceMock);
    }

    [TestMethod]
    public async Task GetCheckList_ShouldReturnCheckListFromCheckListApiServiceFirstTime()
    {
        // Act
        var result = await _dut.GetCheckListAsync(_checkListGuid);

        // Assert
        AssertCheckList(result);
        await _checkListApiServiceMock.Received(1).GetCheckListAsync(_checkListGuid);
    }

    [TestMethod]
    public async Task GetCheckList_ShouldReturnCheckListsFromCacheSecondTime()
    {
        await _dut.GetCheckListAsync(_checkListGuid);

        // Act
        var result = await _dut.GetCheckListAsync(_checkListGuid);

        // Assert
        AssertCheckList(result);
        // since GetCheckListAsync has been called twice, but TryGetCheckListByOidAsync has been called once, the second Get uses cache
        await _checkListApiServiceMock.Received(1).GetCheckListAsync(_checkListGuid);
    }

    private void AssertCheckList(ProCoSys4CheckList checkList)
    {
        Assert.AreEqual(_checkList.IsVoided, checkList.IsVoided);
        Assert.AreEqual(_checkList.ProjectGuid, checkList.ProjectGuid);
        Assert.AreEqual(_checkList.ResponsibleCode, checkList.ResponsibleCode);
    }
}
