using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.History;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemHistory;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Query.Tests.PunchItemQueries.GetPunchItemHistory;

[TestClass]
public class GetPunchItemHistoryQueryHandlerTests : TestsBase
{
    private GetPunchItemHistoryQueryHandler _dut;
    private IHistoryService _historyServiceMock;
    private GetPunchItemHistoryQuery _query;
    private HistoryDto _historyDto;

    [TestInitialize]
    public void Setup()
    {
        _query = new GetPunchItemHistoryQuery(Guid.NewGuid());

        _historyDto = new HistoryDto(
            null,
            _query.PunchItemGuid,
            Guid.NewGuid(),
            DateTime.UtcNow,
            "TestDisplayName",
            "TestFullName",
            new List<PropertyDto>
            {
                new("Property1", "A", "B", "StringAsText")
            },
            "R");
        var historyDtos = new List<HistoryDto>
        {
            _historyDto
        };
        _historyServiceMock = Substitute.For<IHistoryService>();
        _historyServiceMock.GetAllAsync(_query.PunchItemGuid, default)
            .Returns(historyDtos);

        _dut = new GetPunchItemHistoryQueryHandler(_historyServiceMock);
    }

    [TestMethod]
    public async Task HandlingQuery_ShouldReturn_History()
    {
        // Act
        var result = await _dut.Handle(_query, default);

        // Assert
        Assert.IsInstanceOfType(result.Data, typeof(IEnumerable<HistoryDto>));
        var history = result.Data.Single();
        Assert.AreEqual(_historyDto.EventForParentGuid, history.EventForParentGuid);
        Assert.AreEqual(_historyDto.EventForGuid, history.EventForGuid);
        Assert.AreEqual(_historyDto.EventByFullName, history.EventByFullName);
        Assert.AreEqual(_historyDto.EventDisplayName, history.EventDisplayName);
        Assert.AreEqual(_historyDto.Properties.Count, history.Properties.Count);
    }

    [TestMethod]
    public async Task HandlingQuery_Should_CallGetAll_OnHistoryService()
    {
        // Act
        await _dut.Handle(_query, default);

        // Assert
        await _historyServiceMock.Received(1).GetAllAsync(
            _query.PunchItemGuid,
            default);
    }
}
