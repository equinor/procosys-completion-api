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
    private const string Plant = "plant_a";

    private CheckListsByPunchGuidInstance _checkList;

    [TestInitialize]
    public void Setup_OkState()
    {
        _checkList =
            new CheckListsByPunchGuidInstance(
                new SourceCheckListDto(_checkListGuid, "projectA", "FormType", "Resp", "TagFunc"),
                new List<CheckListDto> { new(Guid.NewGuid(), 3, 4, "ASD543", "OK", "TagFunc", "McPkg", "CommPkg") });

        _plantProviderMock = Substitute.For<IPlantProvider>();
        _plantProviderMock.Plant.Returns(Plant);
        _checkListApiServiceMock = Substitute.For<ICheckListApiService>();
        _checkListApiServiceMock.GetByPunchItemGuidAsync(Plant, _punchItemGuid, default).Returns(_checkList);

        _query = new GetCheckListsByPunchItemGuidQuery(_punchItemGuid);
        _dut = new GetCheckListsByPunchItemGuidQueryHandler(_plantProviderMock, _checkListApiServiceMock);
    }

    [TestMethod]
    public async Task HandlingQuery_ShouldReturnCheckListData_By_PunchItemGuid()
    {
        // Act
        var result = await _dut.Handle(_query, default);
        var checkList = result.Data;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.AreEqual(_checkList, checkList);
    }
}

