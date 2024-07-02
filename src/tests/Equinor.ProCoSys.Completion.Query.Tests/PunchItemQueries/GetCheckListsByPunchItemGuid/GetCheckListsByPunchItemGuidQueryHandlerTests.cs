using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetCheckListsByPunchItemGuid;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.Tests.PunchItemQueries.GetCheckListsByPunchItemGuid;

[TestClass]
public class GetCheckListsByPunchItemGuidQueryHandlerTests
{
    private GetCheckListsByPunchItemGuidQuery _query;
    private GetCheckListsByPunchItemGuidQueryHandler _dut;

    private ICheckListApiService _checkListApiServiceMock;
    private IPlantProvider _plantProviderMock;
    private readonly Guid _punchItemGuid = Guid.NewGuid();
    private readonly Guid _checkListGuid = Guid.NewGuid();
    private const string ProjectName = "gooseberry_c";
    private const string Plant = "plant_a";

    [TestInitialize]
    public void Setup_OkState()
    {
        var result = new ChecklistsByPunchGuidInstance(new PICheckListDto(_checkListGuid, ProjectName, string.Empty, string.Empty), 
            new List<CheckListDto>(){new(Guid.NewGuid(), 3, 4, "ASD543", "OK")});

        _plantProviderMock = Substitute.For<IPlantProvider>();
        _checkListApiServiceMock = Substitute.For<ICheckListApiService>();
        _checkListApiServiceMock.GetByPunchItemGuidAsync(Plant, _punchItemGuid).Returns(result);

        _query = new GetCheckListsByPunchItemGuidQuery(_punchItemGuid);
        _dut = new GetCheckListsByPunchItemGuidQueryHandler(_plantProviderMock, _checkListApiServiceMock);
    }

    [TestMethod]
    public async Task HandlingQuery_ShouldReturnCheckListData_By_PunchItemGuid()
    {
        // Act
        var result = await _dut.Handle(_query, default);
        var data = result.Data;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.IsNotNull(data.PunchItemCheckList);
        Assert.AreEqual(_checkListGuid, data.PunchItemCheckList.ProCoSysGuid);
        Assert.IsTrue(_checkListGuid == data.PunchItemCheckList.ProCoSysGuid);
        Assert.IsTrue(1 == data.CheckLists.Count);
        await _checkListApiServiceMock.Received(1).GetByPunchItemGuidAsync(Plant, _punchItemGuid);
    }
}

