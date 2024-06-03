using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.Completion.Infrastructure;
using Equinor.ProCoSys.Completion.Query.History;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Query.Tests.History;

[TestClass]
public class HistoryServiceTests : ReadOnlyTestsBase
{
    private HistoryItem _oldestHistoryItem;
    private HistoryItem _newestHistoryItem;
    private Guid _eventForGuid;

    protected override void SetupNewDatabase(DbContextOptions<CompletionContext> dbContextOptions)
    {
        using var context = new CompletionContext(dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);

        _eventForGuid = Guid.NewGuid();
        var eventAtUtc = DateTime.UtcNow;
        _oldestHistoryItem = new HistoryItem(
            _eventForGuid, 
            "eventDisplayName1", 
            Guid.NewGuid(), 
            "eventByFullName1", 
            eventAtUtc.AddSeconds(1), 
            Guid.NewGuid());
        var property1 = new Property("P1", "D1");
        var property2 = new Property("P2", "D2");
        _oldestHistoryItem.AddProperty(property1);
        _oldestHistoryItem.AddProperty(property2);
        _newestHistoryItem = new HistoryItem(
            _eventForGuid, 
            "eventDisplayName2", 
            Guid.NewGuid(), 
            "eventByFullName2", 
            eventAtUtc.AddSeconds(2));
        context.History.Add(_oldestHistoryItem);
        context.History.Add(_newestHistoryItem);
        context.SaveChangesAsync().Wait();
    }

    [TestMethod]
    public async Task GetAllAsync_ShouldReturnCorrectDtosOrderedDescending()
    {
        // Arrange
        await using var context = new CompletionContext(_dbContextOptions, _plantProviderMock, _eventDispatcherMock, _currentUserProviderMock, _tokenCredentialsMock);
        var dut = new HistoryService(context);

        // Act
        var result = await dut.GetAllAsync(_eventForGuid, default);

        // Assert
        var historyDtos = result.ToList();
        Assert.AreEqual(2, historyDtos.Count);
        AssertHistory(_newestHistoryItem, historyDtos.ElementAt(0));
        AssertHistory(_oldestHistoryItem, historyDtos.ElementAt(1));
    }

    private void AssertHistory(HistoryItem historyItem, HistoryDto historyDto)
    {
        Assert.AreEqual(historyItem.EventForGuid, historyDto.EventForGuid);
        Assert.AreEqual(historyItem.EventAtUtc, historyDto.EventAtUtc);
        Assert.AreEqual(historyItem.EventByFullName, historyDto.EventByFullName);
        Assert.AreEqual(historyItem.EventByOid, historyDto.EventByOid);
        AssertProperties(historyItem, historyDto);

    }

    private static void AssertProperties(HistoryItem historyItem, HistoryDto historyDto)
    {
        Assert.IsNotNull(historyDto.Properties);
        Assert.AreEqual(historyItem.Properties.Count, historyDto.Properties.Count);
        for (var i = 0; i < historyItem.Properties.Count; i++)
        {
            var expectedProperty = historyItem.Properties.ElementAt(i);
            var property = historyDto.Properties.ElementAt(i);
            Assert.AreEqual(expectedProperty.Name, property.Name);
            Assert.AreEqual(expectedProperty.Value, property.Value);
        }
    }
}
