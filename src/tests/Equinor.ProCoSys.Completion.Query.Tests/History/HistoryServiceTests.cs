using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Equinor.ProCoSys.Completion.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Equinor.ProCoSys.Completion.Query.Comments;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Equinor.ProCoSys.Completion.Query.History;

namespace Equinor.ProCoSys.Completion.Query.Tests.History;

[TestClass]
public class HistoryServiceTests : ReadOnlyTestsBase
{
    private HistoryItem _createdHistoryItem;
    private Guid _eventForGuid;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        _eventForGuid = Guid.NewGuid();
        var eventByOid = CurrentUserOid;
        var eventAtUtc = DateTime.UtcNow;
        _createdHistoryItem = new HistoryItem(_eventForGuid, "eventDisplayName", eventByOid, "eventByFullName", eventAtUtc);
        _createdHistoryItem.AddPropertyForCreate("propertyName","propertyValue",ValueDisplayType.StringAsText);
        context.History.Add(_createdHistoryItem);
        context.SaveChangesAsync().Wait();
    }

    [TestMethod]
    public async Task GetAllAsync_ShouldReturnCorrectDtos()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new HistoryService(context);

        // Act
        var result = await dut.GetAllAsync(_eventForGuid, default);

        // Assert
        var historyDtos = result.ToList();
        Assert.AreEqual(1, historyDtos.Count);
        var historyDto = historyDtos.ElementAt(0);
        Assert.AreEqual(_createdHistoryItem.EventForGuid, historyDto.EventForGuid);
        Assert.AreEqual(_createdHistoryItem.EventAtUtc, historyDto.EventAtUtc);
        Assert.AreEqual(_createdHistoryItem.EventByFullName, historyDto.EventByFullName);
        Assert.AreEqual(CurrentUserOid, historyDto.EventByOid);
        AssertProperties(_createdHistoryItem, historyDto);
    }

    private static void AssertProperties(HistoryItem history, HistoryDto historyDto)
    {
        Assert.IsNotNull(historyDto.Properties);
        Assert.AreEqual(history.Properties.Count, historyDto.Properties.Count);
        for (var i = 0; i < history.Properties.Count; i++)
        {
            var expectedProperty = history.Properties.ElementAt(i);
            var property = historyDto.Properties.ElementAt(i);
            Assert.AreEqual(expectedProperty.Name, property.Name);
            Assert.AreEqual(expectedProperty.Value, property.Value);
        }
    }
}
