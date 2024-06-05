using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Equinor.ProCoSys.Completion.WebApi.Synchronization;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Synchronization;

[TestClass]
public class PunchItemChangeHistoryEventConsumerTests
{
    private readonly IHistoryItemRepository _historyItemRepositoryMock = Substitute.For<IHistoryItemRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private PunchItemChangeHistoryEventConsumer _dut = null!;
    private readonly ConsumeContext<PunchItemChangeHistoryEvent> _contextMock = Substitute.For<ConsumeContext<PunchItemChangeHistoryEvent>>();
    private HistoryItem? _historyItemAddedToRepository;

    [TestInitialize]
    public void Setup()
    {
        _dut = new PunchItemChangeHistoryEventConsumer(
            Substitute.For<ILogger<PunchItemChangeHistoryEventConsumer>>(), 
            _historyItemRepositoryMock,
            _unitOfWorkMock);

        _historyItemRepositoryMock
            .When(x => x.Add(Arg.Any<HistoryItem>()))
            .Do(callInfo =>
            {
                _historyItemAddedToRepository = callInfo.Arg<HistoryItem>();
            });
    }
    
    [TestMethod]
    public async Task Consume_ShouldAddNewHistoryItem_WhenHistoryItemDoesNotExist()
    {
        //Arrange
        var bEvent =
            new PunchItemChangeHistoryEvent(Guid.NewGuid(), Guid.NewGuid(), "FN", "old", null, null, null, "by", DateTime.UtcNow);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_historyItemAddedToRepository);
        Assert.AreEqual("'FN' change history imported form old ProCoSys punch item", _historyItemAddedToRepository.EventDisplayName);
        Assert.AreEqual(bEvent.ProCoSysGuid, _historyItemAddedToRepository.Guid);
        Assert.AreEqual(bEvent.PunchItemGuid, _historyItemAddedToRepository.EventForGuid);
        Assert.IsNull(_historyItemAddedToRepository.EventForParentGuid);
        Assert.AreEqual(bEvent.ChangedAt.Ticks, _historyItemAddedToRepository.EventAtUtc.Ticks);
        Assert.AreEqual(bEvent.ChangedBy, _historyItemAddedToRepository.EventByFullName);
        Assert.AreEqual(Guid.Empty, _historyItemAddedToRepository.EventByOid);
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewHistoryItem_ForDescriptionChange()
    {
        //Arrange
        var bEvent =
            new PunchItemChangeHistoryEvent(Guid.NewGuid(), Guid.NewGuid(), "DESCRIPTION", "old", "oldlong", "new", "newlong", "by", DateTime.UtcNow);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_historyItemAddedToRepository);
        Assert.AreEqual("'Description' change history imported form old ProCoSys punch item", _historyItemAddedToRepository.EventDisplayName);
        Assert.AreEqual(1, _historyItemAddedToRepository.Properties.Count);
        AssertProperty(_historyItemAddedToRepository.Properties.ElementAt(0), "Description", bEvent.OldValueLong, bEvent.NewValueLong);
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewHistoryItem_ForActionByChange()
    {
        //Arrange
        var bEvent =
            new PunchItemChangeHistoryEvent(Guid.NewGuid(), Guid.NewGuid(), "ACTIONBYPERSON_ID", "old", "oldlong", "new", "newlong", "by", DateTime.UtcNow);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_historyItemAddedToRepository);
        Assert.AreEqual("'Action by person' change history imported form old ProCoSys punch item", _historyItemAddedToRepository.EventDisplayName);
        Assert.AreEqual(1, _historyItemAddedToRepository.Properties.Count);
        AssertProperty(_historyItemAddedToRepository.Properties.ElementAt(0), "Action by person", bEvent.OldValue, bEvent.NewValue);
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewHistoryItem_ForRaisedChange()
    {
        //Arrange
        var bEvent =
            new PunchItemChangeHistoryEvent(Guid.NewGuid(), Guid.NewGuid(), "CLEARED", "old", "oldlong", "new", "newlong", "by", DateTime.UtcNow);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_historyItemAddedToRepository);
        Assert.AreEqual("'Cleared' change history imported form old ProCoSys punch item", _historyItemAddedToRepository.EventDisplayName);
        Assert.AreEqual(1, _historyItemAddedToRepository.Properties.Count);
        AssertProperty(_historyItemAddedToRepository.Properties.ElementAt(0), "Cleared", bEvent.OldValue, bEvent.NewValue);
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewHistoryItem_ForVerifiedChange()
    {
        //Arrange
        var bEvent =
            new PunchItemChangeHistoryEvent(Guid.NewGuid(), Guid.NewGuid(), "VERIFIED", "old", "oldlong", "new", "newlong", "by", DateTime.UtcNow);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_historyItemAddedToRepository);
        Assert.AreEqual("'Verified' change history imported form old ProCoSys punch item", _historyItemAddedToRepository.EventDisplayName);
        Assert.AreEqual(1, _historyItemAddedToRepository.Properties.Count);
        AssertProperty(_historyItemAddedToRepository.Properties.ElementAt(0), "Verified", bEvent.OldValue, bEvent.NewValue);
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewHistoryItem_ForRejectedChange()
    {
        //Arrange
        var bEvent =
            new PunchItemChangeHistoryEvent(Guid.NewGuid(), Guid.NewGuid(), "REJECTED", "old", "oldlong", "new", "newlong", "by", DateTime.UtcNow);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_historyItemAddedToRepository);
        Assert.AreEqual("'Rejected' change history imported form old ProCoSys punch item", _historyItemAddedToRepository.EventDisplayName);
        Assert.AreEqual(1, _historyItemAddedToRepository.Properties.Count);
        AssertProperty(_historyItemAddedToRepository.Properties.ElementAt(0), "Rejected", bEvent.OldValue, bEvent.NewValue);
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewHistoryItem_ForClearedByOrgChange()
    {
        //Arrange
        var bEvent =
            new PunchItemChangeHistoryEvent(Guid.NewGuid(), Guid.NewGuid(), "CLEAREDBYORG_ID", "old", "oldlong", "new", "newlong", "by", DateTime.UtcNow);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_historyItemAddedToRepository);
        Assert.AreEqual("'Clearing by org.' change history imported form old ProCoSys punch item", _historyItemAddedToRepository.EventDisplayName);
        Assert.AreEqual(1, _historyItemAddedToRepository.Properties.Count);
        AssertProperty(_historyItemAddedToRepository.Properties.ElementAt(0), 
            "Clearing by org.", 
            $"{bEvent.OldValue}, {bEvent.OldValueLong}",
            $"{bEvent.NewValue}, {bEvent.NewValueLong}");
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewHistoryItem_ForRaisedByOrgChange()
    {
        //Arrange
        var bEvent =
            new PunchItemChangeHistoryEvent(Guid.NewGuid(), Guid.NewGuid(), "RAISEDBYORG_ID", "old", "oldlong", "new", "newlong", "by", DateTime.UtcNow);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_historyItemAddedToRepository);
        Assert.AreEqual("'Raised by org.' change history imported form old ProCoSys punch item", _historyItemAddedToRepository.EventDisplayName);
        Assert.AreEqual(1, _historyItemAddedToRepository.Properties.Count);
        AssertProperty(_historyItemAddedToRepository.Properties.ElementAt(0),
            "Raised by org.", 
            $"{bEvent.OldValue}, {bEvent.OldValueLong}",
            $"{bEvent.NewValue}, {bEvent.NewValueLong}");
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewHistoryItem_ForPunchSortingChange()
    {
        //Arrange
        var bEvent =
            new PunchItemChangeHistoryEvent(Guid.NewGuid(), Guid.NewGuid(), "PUNCHLISTSORTING_ID", "old", "oldlong", "new", "newlong", "by", DateTime.UtcNow);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_historyItemAddedToRepository);
        Assert.AreEqual("'Punch sorting' change history imported form old ProCoSys punch item", _historyItemAddedToRepository.EventDisplayName);
        Assert.AreEqual(1, _historyItemAddedToRepository.Properties.Count);
        AssertProperty(_historyItemAddedToRepository.Properties.ElementAt(0),
            "Punch sorting",
            $"{bEvent.OldValue}, {bEvent.OldValueLong}",
            $"{bEvent.NewValue}, {bEvent.NewValueLong}");
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewHistoryItem_ForPunchTypeChange()
    {
        //Arrange
        var bEvent =
            new PunchItemChangeHistoryEvent(Guid.NewGuid(), Guid.NewGuid(), "PUNCHLISTTYPE_ID", "old", "oldlong", "new", "newlong", "by", DateTime.UtcNow);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_historyItemAddedToRepository);
        Assert.AreEqual("'Punch type' change history imported form old ProCoSys punch item", _historyItemAddedToRepository.EventDisplayName);
        Assert.AreEqual(1, _historyItemAddedToRepository.Properties.Count);
        AssertProperty(_historyItemAddedToRepository.Properties.ElementAt(0),
            "Punch type",
            $"{bEvent.OldValue}, {bEvent.OldValueLong}",
            $"{bEvent.NewValue}, {bEvent.NewValueLong}");
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewHistoryItem_ForPriorityChange()
    {
        //Arrange
        var bEvent =
            new PunchItemChangeHistoryEvent(Guid.NewGuid(), Guid.NewGuid(), "PRIORITY_ID", "old", "oldlong", "new", "newlong", "by", DateTime.UtcNow);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_historyItemAddedToRepository);
        Assert.AreEqual("'Punch priority' change history imported form old ProCoSys punch item", _historyItemAddedToRepository.EventDisplayName);
        Assert.AreEqual(1, _historyItemAddedToRepository.Properties.Count);
        AssertProperty(_historyItemAddedToRepository.Properties.ElementAt(0),
            "Punch priority",
            $"{bEvent.OldValue}, {bEvent.OldValueLong}",
            $"{bEvent.NewValue}, {bEvent.NewValueLong}");
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewHistoryItem_ForCategoryChange()
    {
        //Arrange
        var bEvent =
            new PunchItemChangeHistoryEvent(Guid.NewGuid(), Guid.NewGuid(), "STATUS_ID", "old", "oldlong", "new", "newlong", "by", DateTime.UtcNow);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_historyItemAddedToRepository);
        Assert.AreEqual("'Category' change history imported form old ProCoSys punch item", _historyItemAddedToRepository.EventDisplayName);
        Assert.AreEqual(1, _historyItemAddedToRepository.Properties.Count);
        AssertProperty(_historyItemAddedToRepository.Properties.ElementAt(0),
            "Category", bEvent.OldValue, bEvent.NewValue);
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewHistoryItem_ForDocumentChange()
    {
        //Arrange
        var bEvent =
            new PunchItemChangeHistoryEvent(Guid.NewGuid(), Guid.NewGuid(), "DRAWING_ID", "old", "oldlong", "new", "newlong", "by", DateTime.UtcNow);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_historyItemAddedToRepository);
        Assert.AreEqual("'Document no' change history imported form old ProCoSys punch item", _historyItemAddedToRepository.EventDisplayName);
        Assert.AreEqual(1, _historyItemAddedToRepository.Properties.Count);
        AssertProperty(_historyItemAddedToRepository.Properties.ElementAt(0),
            "Document no", bEvent.OldValue, bEvent.NewValue);
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewHistoryItem_ForDueDateChange()
    {
        //Arrange
        var bEvent =
            new PunchItemChangeHistoryEvent(Guid.NewGuid(), Guid.NewGuid(), "DUEDATE", "old", "oldlong", "new", "newlong", "by", DateTime.UtcNow);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_historyItemAddedToRepository);
        Assert.AreEqual("'Due date' change history imported form old ProCoSys punch item", _historyItemAddedToRepository.EventDisplayName);
        Assert.AreEqual(1, _historyItemAddedToRepository.Properties.Count);
        AssertProperty(_historyItemAddedToRepository.Properties.ElementAt(0),
            "Due date", bEvent.OldValue, bEvent.NewValue);
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewHistoryItem_ForEstimateChange()
    {
        //Arrange
        var bEvent =
            new PunchItemChangeHistoryEvent(Guid.NewGuid(), Guid.NewGuid(), "ESTIMATE", "old", "oldlong", "new", "newlong", "by", DateTime.UtcNow);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_historyItemAddedToRepository);
        Assert.AreEqual("'Estimate' change history imported form old ProCoSys punch item", _historyItemAddedToRepository.EventDisplayName);
        Assert.AreEqual(1, _historyItemAddedToRepository.Properties.Count);
        AssertProperty(_historyItemAddedToRepository.Properties.ElementAt(0),
            "Estimate", bEvent.OldValue, bEvent.NewValue);
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewHistoryItem_ForOriginalWOChange()
    {
        //Arrange
        var bEvent =
            new PunchItemChangeHistoryEvent(Guid.NewGuid(), Guid.NewGuid(), "ORIGINALWO_ID", "old", "oldlong", "new", "newlong", "by", DateTime.UtcNow);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_historyItemAddedToRepository);
        Assert.AreEqual("'Original WO no' change history imported form old ProCoSys punch item", _historyItemAddedToRepository.EventDisplayName);
        Assert.AreEqual(1, _historyItemAddedToRepository.Properties.Count);
        AssertProperty(_historyItemAddedToRepository.Properties.ElementAt(0),
            "Original WO no", bEvent.OldValue, bEvent.NewValue);
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewHistoryItem_ForWOChange()
    {
        //Arrange
        var bEvent =
            new PunchItemChangeHistoryEvent(Guid.NewGuid(), Guid.NewGuid(), "WO_ID", "old", "oldlong", "new", "newlong", "by", DateTime.UtcNow);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_historyItemAddedToRepository);
        Assert.AreEqual("'WO no' change history imported form old ProCoSys punch item", _historyItemAddedToRepository.EventDisplayName);
        Assert.AreEqual(1, _historyItemAddedToRepository.Properties.Count);
        AssertProperty(_historyItemAddedToRepository.Properties.ElementAt(0),
            "WO no", bEvent.OldValue, bEvent.NewValue);
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewHistoryItem_ForSWCRChange()
    {
        //Arrange
        var bEvent =
            new PunchItemChangeHistoryEvent(Guid.NewGuid(), Guid.NewGuid(), "SWCR_ID", "old", "oldlong", "new", "newlong", "by", DateTime.UtcNow);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_historyItemAddedToRepository);
        Assert.AreEqual("'SWCR no' change history imported form old ProCoSys punch item", _historyItemAddedToRepository.EventDisplayName);
        Assert.AreEqual(1, _historyItemAddedToRepository.Properties.Count);
        AssertProperty(_historyItemAddedToRepository.Properties.ElementAt(0),
            "SWCR no", bEvent.OldValue, bEvent.NewValue);
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewHistoryItem_WhenEventHasUnspecifiedDateTimeKind()
    {
        //Arrange
        var changedAtUnspecifiedKind = new DateTime(1999, 2, 1, 12, 1, 2, DateTimeKind.Unspecified);
        var bEvent =
            new PunchItemChangeHistoryEvent(Guid.NewGuid(), Guid.NewGuid(), "FN", "old", null, null, null, "by", changedAtUnspecifiedKind);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_historyItemAddedToRepository);
        Assert.AreEqual(bEvent.ChangedAt.Ticks, _historyItemAddedToRepository.EventAtUtc.Ticks);
    }

    [TestMethod]
    public async Task Consume_ShouldNotAddNewHistoryItem_WhenHistoryItemExists()
    {
        //Arrange
        var bEvent =
            new PunchItemChangeHistoryEvent(Guid.NewGuid(), Guid.NewGuid(), "FN", "old", null, null, null, "by", DateTime.UtcNow);
        _historyItemRepositoryMock.ExistsAsync(bEvent.ProCoSysGuid, default).Returns(true);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNull(_historyItemAddedToRepository);
        await _unitOfWorkMock.Received(0).SaveChangesAsync();
    }

    [TestMethod]
    public async Task Consume_ShouldThrowException_IfNoProCoSysGuid()
    {
        //Arrange
        var bEvent =
            new PunchItemChangeHistoryEvent(Guid.Empty, Guid.NewGuid(), "FN", "old", null, null, null, "by", DateTime.UtcNow);
        _contextMock.Message.Returns(bEvent);

        //Act and Assert
        await Assert.ThrowsExceptionAsync<Exception>(()
            => _dut.Consume(_contextMock), "Message is missing ProCoSysGuid");
    }

    [TestMethod]
    public async Task Consume_ShouldAddNewHistoryItem_WhenBothOldAndNewValueIsNull()
    {
        //Arrange
        var bEvent =
            new PunchItemChangeHistoryEvent(Guid.NewGuid(), Guid.NewGuid(), "WO_ID", null, null, null, null, "by", DateTime.UtcNow);
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        Assert.IsNotNull(_historyItemAddedToRepository);
        Assert.AreEqual("'WO no' change history imported form old ProCoSys punch item", _historyItemAddedToRepository.EventDisplayName);
        Assert.AreEqual(1, _historyItemAddedToRepository.Properties.Count);
        AssertProperty(_historyItemAddedToRepository.Properties.ElementAt(0),
            "WO no", null, null);
    }

    private void AssertProperty(Property property, string fieldName, string? oldValue, string? newValue)
    {
        Assert.AreEqual(fieldName, property.Name);
        Assert.AreEqual(oldValue, property.OldValue);
        Assert.AreEqual(newValue, property.Value);
        Assert.AreEqual(ValueDisplayType.StringAsText.ToString(), property.ValueDisplayType);
        Assert.IsNull(property.OldOidValue);
        Assert.IsNull(property.OidValue);
    }
}
