using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DuplicatePunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.AttachmentEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.DuplicatePunchItem;

[TestClass]
public class DuplicatePunchItemCommandHandlerTests : PunchItemCommandTestsBase
{
    private DuplicatePunchItemCommand _commandWithoutCopyAttachments;
    private DuplicatePunchItemCommand _commandWithCopyAttachments;
    private DuplicatePunchItemCommandHandler _dut;
    private readonly Guid _checkListGuid1 = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private readonly Guid _checkListGuid2 = new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private readonly IAttachmentRepository _attachmentRepositoryMock = Substitute.For<IAttachmentRepository>();
    private readonly IAttachmentService _attachmentServiceMock = Substitute.For<IAttachmentService>();
    private readonly List<PunchItem> _punchItemsAddedToRepository = [];
    private int _punchItemId = 7;
    private PunchItem _punchItemToDuplicate;

    [TestInitialize]
    public void Setup()
    {
        _punchItemToDuplicate = _existingPunchItem[TestPlantA];
        _commandWithoutCopyAttachments = new DuplicatePunchItemCommand(
            _punchItemToDuplicate.Guid,
            [_checkListGuid1, _checkListGuid2],
            false)
        {
            PunchItem = _punchItemToDuplicate
        };

        _commandWithCopyAttachments = new DuplicatePunchItemCommand(
            _punchItemToDuplicate.Guid,
            [_checkListGuid1, _checkListGuid2],
            true)
        {
            PunchItem = _punchItemToDuplicate
        };

        _punchItemRepositoryMock
            .When(x => x.Add(Arg.Any<PunchItem>()))
            .Do(callInfo =>
            {
                _punchItemsAddedToRepository.Add(callInfo.Arg<PunchItem>());
            });

        _unitOfWorkMock
            .When(x => x.SaveChangesAsync())
            .Do(_ =>
            {
                var punchItemAddedToRepository = _punchItemsAddedToRepository.Last();
                punchItemAddedToRepository.SetCreated(_currentPerson);
                punchItemAddedToRepository.SetProtectedIdForTesting(_punchItemId++);
            });

        _dut = new DuplicatePunchItemCommandHandler(
            _punchItemRepositoryMock,
            _attachmentRepositoryMock,
            _attachmentServiceMock,
            _syncToPCS4ServiceMock,
            _unitOfWorkMock,
            _messageProducerMock,
            _checkListApiServiceMock,
            Substitute.For<ILogger<DuplicatePunchItemCommandHandler>>());
    }

    [TestMethod]
    public async Task HandlingCommand_WithAllValues_ShouldAddCorrectPunchItem_ToPunchItemRepository()
    {
        // Arrange
        FillAllProperties(_punchItemToDuplicate);

        // Act
        await _dut.Handle(_commandWithoutCopyAttachments, default);

        // Assert
        AssertAllPropertiesInPunchItem(_punchItemsAddedToRepository.ElementAt(0));
        AssertAllPropertiesInPunchItem(_punchItemsAddedToRepository.ElementAt(1));
    }

    private void AssertAllPropertiesInPunchItem(PunchItem duplicatedPunchItem)
    {
        Assert.IsNotNull(duplicatedPunchItem);
        Assert.AreEqual(_punchItemToDuplicate.Description, duplicatedPunchItem.Description);
        Assert.AreEqual(_punchItemToDuplicate.ProjectId, duplicatedPunchItem.ProjectId);
        Assert.AreEqual(_punchItemToDuplicate.RaisedByOrgId, duplicatedPunchItem.RaisedByOrgId);
        Assert.AreEqual(_punchItemToDuplicate.ClearingByOrgId, duplicatedPunchItem.ClearingByOrgId);
        Assert.AreEqual(_punchItemToDuplicate.ActionById, duplicatedPunchItem.ActionById);
        Assert.AreEqual(_punchItemToDuplicate.DueTimeUtc, duplicatedPunchItem.DueTimeUtc);
        Assert.AreEqual(_punchItemToDuplicate.PriorityId, duplicatedPunchItem.PriorityId);
        Assert.AreEqual(_punchItemToDuplicate.SortingId, duplicatedPunchItem.SortingId);
        Assert.AreEqual(_punchItemToDuplicate.TypeId, duplicatedPunchItem.TypeId);
        Assert.AreEqual(_punchItemToDuplicate.Estimate, duplicatedPunchItem.Estimate);
        Assert.AreEqual(_punchItemToDuplicate.OriginalWorkOrderId, duplicatedPunchItem.OriginalWorkOrderId);
        Assert.AreEqual(_punchItemToDuplicate.WorkOrderId, duplicatedPunchItem.WorkOrderId);
        Assert.AreEqual(_punchItemToDuplicate.SWCRId, duplicatedPunchItem.SWCRId);
        Assert.AreEqual(_punchItemToDuplicate.DocumentId, duplicatedPunchItem.DocumentId);
        // ExternalItemNo is not copied when duplicating - it should only be set during creation
        Assert.IsNull(duplicatedPunchItem.ExternalItemNo);
        Assert.AreEqual(_punchItemToDuplicate.MaterialRequired, duplicatedPunchItem.MaterialRequired);
        Assert.AreEqual(_punchItemToDuplicate.MaterialETAUtc, duplicatedPunchItem.MaterialETAUtc);
        Assert.AreEqual(_punchItemToDuplicate.MaterialExternalNo, duplicatedPunchItem.MaterialExternalNo);
    }

    [TestMethod]
    public async Task HandlingCommand_WithRequiredValues_ShouldAddCorrectPunchItem_ToPunchItemRepository()
    {
        // Act
        await _dut.Handle(_commandWithoutCopyAttachments, default);

        // Assert
        AssertRequiredPropertiesInPunchItem(_punchItemsAddedToRepository.ElementAt(0));
        AssertRequiredPropertiesInPunchItem(_punchItemsAddedToRepository.ElementAt(1));
    }

    private void AssertRequiredPropertiesInPunchItem(PunchItem duplicatedPunchItem)
    {
        Assert.IsNotNull(duplicatedPunchItem);
        Assert.AreEqual(_punchItemToDuplicate.Description, duplicatedPunchItem.Description);
        Assert.AreEqual(_punchItemToDuplicate.ProjectId, duplicatedPunchItem.ProjectId);
        Assert.AreEqual(_punchItemToDuplicate.RaisedByOrgId, duplicatedPunchItem.RaisedByOrgId);
        Assert.AreEqual(_punchItemToDuplicate.ClearingByOrgId, duplicatedPunchItem.ClearingByOrgId);
        Assert.IsFalse(duplicatedPunchItem.ActionById.HasValue);
        Assert.IsFalse(duplicatedPunchItem.DueTimeUtc.HasValue);
        Assert.IsFalse(duplicatedPunchItem.PriorityId.HasValue);
        Assert.IsFalse(duplicatedPunchItem.SortingId.HasValue);
        Assert.IsFalse(duplicatedPunchItem.TypeId.HasValue);
        Assert.IsFalse(duplicatedPunchItem.Estimate.HasValue);
        Assert.IsFalse(duplicatedPunchItem.OriginalWorkOrderId.HasValue);
        Assert.IsFalse(duplicatedPunchItem.WorkOrderId.HasValue);
        Assert.IsFalse(duplicatedPunchItem.SWCRId.HasValue);
        Assert.IsFalse(duplicatedPunchItem.DocumentId.HasValue);
        Assert.IsNull(duplicatedPunchItem.ExternalItemNo);
        Assert.IsFalse(duplicatedPunchItem.MaterialRequired);
        Assert.IsFalse(duplicatedPunchItem.MaterialETAUtc.HasValue);
        Assert.IsNull(duplicatedPunchItem.MaterialExternalNo);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotGetAttachments_WhenNotCopyAttachments()
    {
        // Act
        await _dut.Handle(_commandWithoutCopyAttachments, CancellationToken.None);

        // Assert
        await _attachmentRepositoryMock.DidNotReceive().GetAllByParentGuidAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldGetAttachments_WhenCopyAttachments()
    {
        // Arrange
        SetupAttachmentDuplication(_commandWithCopyAttachments);

        // Act
        await _dut.Handle(_commandWithCopyAttachments, CancellationToken.None);

        // Assert
        await _attachmentRepositoryMock.Received(1).GetAllByParentGuidAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSave()
    {
        // Act
        await _dut.Handle(_commandWithoutCopyAttachments, CancellationToken.None);

        // Assert
        await _unitOfWorkMock.Received(3).SaveChangesAsync();
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldPublishPunchItemCreatedIntegrationEvent()
    {
        // Arrange
        FillAllProperties(_punchItemToDuplicate);

        List<PunchItemCreatedIntegrationEvent> integrationEvents = [];
        _messageProducerMock
            .When(x => x.PublishAsync(Arg.Any<PunchItemCreatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(callbackInfo =>
            {
                integrationEvents.Add(callbackInfo.Arg<PunchItemCreatedIntegrationEvent>());
            });

        // Act
        await _dut.Handle(_commandWithoutCopyAttachments, CancellationToken.None);

        // Assert
        for (var i = 0; i < integrationEvents.Count; i++)
        {
            var integrationEvent = integrationEvents.ElementAt(i);
            var punchItem = _punchItemsAddedToRepository.ElementAt(i);
            Assert.IsNotNull(integrationEvent);
            AssertRequiredProperties(punchItem, integrationEvent);
            AssertOptionalProperties(punchItem, integrationEvent);
            AssertNotCleared(integrationEvent);
            AssertNotRejected(integrationEvent);
            AssertNotVerified(integrationEvent);
        }
    }

    [TestMethod]
    public async Task HandlingCommand_WithAllPropertiesSet_ShouldSendHistoryCreatedIntegrationEvent()
    {
        // Arrange
        FillAllProperties(_punchItemToDuplicate);
        List<HistoryCreatedIntegrationEvent> historyEvents = [];
        _messageProducerMock
            .When(x => x.SendHistoryAsync(
                Arg.Any<HistoryCreatedIntegrationEvent>(),
                Arg.Any<CancellationToken>()))
            .Do(callbackInfo =>
            {
                historyEvents.Add(callbackInfo.Arg<HistoryCreatedIntegrationEvent>());
            });

        // Act
        await _dut.Handle(_commandWithoutCopyAttachments, CancellationToken.None);

        // Assert
        for (var i = 0; i < historyEvents.Count; i++)
        {
            var historyEvent = historyEvents.ElementAt(i);
            var punchItem = _punchItemsAddedToRepository.ElementAt(i);

            AssertHistoryCreatedIntegrationEvent(
                historyEvent,
                $"Punch item {punchItem.Category} {punchItem.ItemNo} duplicated from {_punchItemToDuplicate.ItemNo}",
                punchItem.CheckListGuid,
                punchItem,
                punchItem);

            var properties = historyEvent.Properties;
            Assert.IsNotNull(properties);
            Assert.AreEqual(18, properties.Count);
            AssertProperty(
                properties
                    .SingleOrDefault(c => c.Name == nameof(PunchItem.ItemNo)),
                punchItem.ItemNo,
                ValueDisplayType.IntAsText);
            AssertProperty(
                properties
                    .SingleOrDefault(c => c.Name == nameof(PunchItem.Category)),
                punchItem.Category.ToString());
            AssertProperty(
                properties
                    .SingleOrDefault(c => c.Name == nameof(PunchItem.Description)),
                punchItem.Description);
            AssertProperty(
                properties
                    .SingleOrDefault(c => c.Name == nameof(PunchItem.RaisedByOrg)),
                punchItem.RaisedByOrg.ToString());
            AssertProperty(
                properties
                    .SingleOrDefault(c => c.Name == nameof(PunchItem.ClearingByOrg)),
                punchItem.ClearingByOrg.ToString());
            AssertPerson(
                properties
                    .SingleOrDefault(c => c.Name == nameof(PunchItem.ActionBy)),
                new User(_existingPerson1.Guid, _existingPerson1.GetFullName()));
            AssertProperty(
                properties
                    .SingleOrDefault(c => c.Name == nameof(PunchItem.DueTimeUtc)),
                punchItem.DueTimeUtc,
                ValueDisplayType.DateTimeAsDateOnly);
            AssertProperty(
                properties
                    .SingleOrDefault(c => c.Name == nameof(PunchItem.Priority)),
                punchItem.Priority!.ToString());
            AssertProperty(
                properties
                    .SingleOrDefault(c => c.Name == nameof(PunchItem.Sorting)),
                punchItem.Sorting!.ToString());
            AssertProperty(
                properties
                    .SingleOrDefault(c => c.Name == nameof(PunchItem.Type)),
                punchItem.Type!.ToString());
            AssertProperty(
                properties
                    .SingleOrDefault(c => c.Name == nameof(PunchItem.Estimate)),
                punchItem.Estimate!.Value,
                ValueDisplayType.IntAsText);

            AssertProperty(
                properties
                    .SingleOrDefault(c => c.Name == nameof(PunchItem.Document)),
                punchItem.Document!.No);
            AssertProperty(
                properties
                    .SingleOrDefault(c => c.Name == nameof(PunchItem.SWCR)),
                punchItem.SWCR!.No,
                ValueDisplayType.IntAsText);
            AssertProperty(
                properties
                    .SingleOrDefault(c => c.Name == nameof(PunchItem.OriginalWorkOrder)),
                punchItem.OriginalWorkOrder!.No);
            AssertProperty(
                properties
                    .SingleOrDefault(c => c.Name == nameof(PunchItem.WorkOrder)),
                punchItem.WorkOrder!.No);

            // ExternalItemNo is not copied when duplicating - removed from history properties
            AssertProperty(
                properties
                    .SingleOrDefault(c => c.Name == nameof(PunchItem.MaterialRequired)),
                punchItem.MaterialRequired,
                ValueDisplayType.BoolAsYesNo);
            AssertProperty(
                properties
                    .SingleOrDefault(c => c.Name == nameof(PunchItem.MaterialETAUtc)),
                punchItem.MaterialETAUtc!.Value,
                ValueDisplayType.DateTimeAsDateOnly);
            AssertProperty(
                properties
                    .SingleOrDefault(c => c.Name == nameof(PunchItem.MaterialExternalNo)),
                punchItem.MaterialExternalNo);
        }
    }

    [TestMethod]
    public async Task HandlingCommand_WithOnlyRequiredPropertiesSet_ShouldSendHistoryCreatedIntegrationEvent()
    {
        // Arrange
        List<HistoryCreatedIntegrationEvent> historyEvents = [];
        _messageProducerMock
            .When(x => x.SendHistoryAsync(
                Arg.Any<HistoryCreatedIntegrationEvent>(),
                Arg.Any<CancellationToken>()))
            .Do(callbackInfo =>
            {
                historyEvents.Add(callbackInfo.Arg<HistoryCreatedIntegrationEvent>());
            });

        // Act
        await _dut.Handle(_commandWithoutCopyAttachments, CancellationToken.None);

        // Assert
        for (var i = 0; i < historyEvents.Count; i++)
        {
            var historyEvent = historyEvents.ElementAt(i);
            var punchItem = _punchItemsAddedToRepository.ElementAt(i);

            // Assert
            AssertHistoryCreatedIntegrationEvent(
                historyEvent,
                $"Punch item {punchItem.Category} {punchItem.ItemNo} duplicated from {_punchItemToDuplicate.ItemNo}",
                punchItem.CheckListGuid,
                punchItem,
                punchItem);
            var properties = historyEvent.Properties;

            Assert.IsNotNull(properties);
            Assert.AreEqual(5, properties.Count);
            AssertProperty(
                properties
                    .SingleOrDefault(c => c.Name == nameof(PunchItem.ItemNo)),
                punchItem.ItemNo,
                ValueDisplayType.IntAsText);
            AssertProperty(
                properties
                    .SingleOrDefault(c => c.Name == nameof(PunchItem.Category)),
                punchItem.Category.ToString());
            AssertProperty(
                properties
                    .SingleOrDefault(c => c.Name == nameof(PunchItem.Description)),
                punchItem.Description);
            AssertProperty(
                properties
                    .SingleOrDefault(c => c.Name == nameof(PunchItem.RaisedByOrg)),
                punchItem.RaisedByOrg.ToString());
            AssertProperty(
                properties
                    .SingleOrDefault(c => c.Name == nameof(PunchItem.ClearingByOrg)),
                punchItem.ClearingByOrg.ToString());
        }
    }

    #region Unit Tests which can be removed when no longer sync to pcs4
    [TestMethod]
    public async Task HandlingCommand_ShouldRecalculateChecklist()
    {
        // Act
        await _dut.Handle(_commandWithoutCopyAttachments, CancellationToken.None);

        // Assert
        await _checkListApiServiceMock.Received(1).RecalculateCheckListStatusForManyAsync(
            TestPlantA, 
            Arg.Is<List<Guid>>(guids => guids.Count == 2 && 
                                        guids.Contains(_checkListGuid1) && 
                                        guids.Contains(_checkListGuid2)), 
            Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSyncNewPunchItemsWithPcs4()
    {
        // Arrange
        List<PunchItemCreatedIntegrationEvent> integrationEvents = [];
        _messageProducerMock
            .When(x => x.PublishAsync(Arg.Any<PunchItemCreatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(callbackInfo =>
            {
                integrationEvents.Add(callbackInfo.Arg<PunchItemCreatedIntegrationEvent>());
            });

        // Act
        await _dut.Handle(_commandWithoutCopyAttachments, CancellationToken.None);

        // Assert
        Assert.AreEqual(2, integrationEvents.Count);
        await _syncToPCS4ServiceMock.Received(1).SyncNewPunchListItemAsync(integrationEvents.ElementAt(0), Arg.Any<CancellationToken>());
        await _syncToPCS4ServiceMock.Received(1).SyncNewPunchListItemAsync(integrationEvents.ElementAt(1), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldBeginTransaction()
    {
        // Act
        await _dut.Handle(_commandWithoutCopyAttachments, CancellationToken.None);

        // Assert
        await _unitOfWorkMock.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldCommitTransaction_WhenNoExceptions()
    {
        // Act
        await _dut.Handle(_commandWithoutCopyAttachments, CancellationToken.None);

        // Assert
        await _unitOfWorkMock.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldRollbackTransaction_WhenExceptionThrown()
    {
        // Arrange
        _unitOfWorkMock
            .When(u => u.SaveChangesAsync())
            .Do(_ => throw new Exception());

        // Act
        var exception = await Assert.ThrowsExceptionAsync<Exception>(
            () => _dut.Handle(_commandWithoutCopyAttachments, CancellationToken.None));

        // Assert
        await _unitOfWorkMock.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
        Assert.IsInstanceOfType(exception, typeof(Exception));
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotSyncWithPcs4_WhenSavingChangesFails()
    {
        // Arrange
        _unitOfWorkMock.When(x => x.SaveChangesAsync())
           .Do(_ => throw new Exception("SaveChangesAsync error"));

        // Act
        await Assert.ThrowsExceptionAsync<Exception>(async () =>
        {
            await _dut.Handle(_commandWithoutCopyAttachments, CancellationToken.None);
        });

        // Assert
        await _syncToPCS4ServiceMock.DidNotReceive().SyncNewPunchListItemAsync(Arg.Any<object>(), Arg.Any<CancellationToken>());
        await _syncToPCS4ServiceMock.DidNotReceive().SyncNewAttachmentAsync(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotRecalculate_WhenSavingChangesFails()
    {
        // Arrange
        _unitOfWorkMock.When(x => x.SaveChangesAsync())
           .Do(_ => throw new Exception("SaveChangesAsync error"));

        // Act
        await Assert.ThrowsExceptionAsync<Exception>(async () =>
        {
            await _dut.Handle(_commandWithoutCopyAttachments, CancellationToken.None);
        });

        // Assert
        await _checkListApiServiceMock.DidNotReceive()
            .RecalculateCheckListStatusForManyAsync(Arg.Any<string>(), Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotRecalculate_WhenSyncingPunchItemWithPcs4Fails()
    {
        // Arrange
        _syncToPCS4ServiceMock.When(x => x.SyncNewPunchListItemAsync(Arg.Any<object>(), Arg.Any<CancellationToken>()))
            .Do(_ => throw new Exception("SyncNewPunchListItemAsync error"));

        // Act
        await _dut.Handle(_commandWithoutCopyAttachments, CancellationToken.None);

        // Assert
        await _checkListApiServiceMock.DidNotReceive()
            .RecalculateCheckListStatusForManyAsync(Arg.Any<string>(), Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotSyncAttachments_WhenSyncingPunchItemWithPcs4Fails()
    {
        // Arrange
        _syncToPCS4ServiceMock.When(x => x.SyncNewPunchListItemAsync(Arg.Any<object>(), Arg.Any<CancellationToken>()))
            .Do(_ => throw new Exception("SyncNewPunchListItemAsync error"));

        // Act
        await _dut.Handle(_commandWithoutCopyAttachments, CancellationToken.None);

        // Assert
        await _syncToPCS4ServiceMock.DidNotReceive().SyncNewAttachmentAsync(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotSyncAttachments_WhenRecalculatingFails()
    {
        // Arrange
        _checkListApiServiceMock
            .When(x => x.RecalculateCheckListStatusForManyAsync(Arg.Any<string>(), Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>()))
            .Do(_ => throw new Exception("RecalculateCheckListStatusForMany error"));

        // Act
        await _dut.Handle(_commandWithoutCopyAttachments, CancellationToken.None);

        // Assert
        await _syncToPCS4ServiceMock.DidNotReceive().SyncNewAttachmentAsync(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotThrowError_WhenSyncingPunchItemWithPcs4Fails()
    {
        // Arrange
        _syncToPCS4ServiceMock.When(x => x.SyncNewPunchListItemAsync(Arg.Any<object>(), Arg.Any<CancellationToken>()))
            .Do(_ => throw new Exception("SyncNewPunchListItemAsync error"));

        // Act and Assert
        try
        {
            await _dut.Handle(_commandWithoutCopyAttachments, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Assert.Fail("Excepted no exception, but got: " + ex.Message);
        }
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotThrowError_WhenRecalculatingFails()
    {
        // Arrange
        _checkListApiServiceMock
            .When(x => x.RecalculateCheckListStatusForManyAsync(Arg.Any<string>(), Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>()))
            .Do(_ => throw new Exception("RecalculateCheckListStatusForMany error"));

        // Act and Assert
        try
        {
            await _dut.Handle(_commandWithoutCopyAttachments, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Assert.Fail("Excepted no exception, but got: " + ex.Message);
        }
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotThrowError_WhenSyncingAttachmentsWithPcs4Fails()
    {
        // Arrange
        _syncToPCS4ServiceMock.When(x => x.SyncNewAttachmentAsync(Arg.Any<object>(), Arg.Any<CancellationToken>()))
            .Do(_ => throw new Exception("SyncNewAttachmentAsync error"));

        // Act and Assert
        try
        {
            await _dut.Handle(_commandWithoutCopyAttachments, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Assert.Fail("Excepted no exception, but got: " + ex.Message);
        }
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSyncNewAttachmentsWithPcs4_WhenPunchHasAttachments_AndCopy()
    {
        // Arrange
        var attachmentCreatedIntegrationEvents = SetupAttachmentDuplication(_commandWithCopyAttachments);

        // Act
        await _dut.Handle(_commandWithCopyAttachments, CancellationToken.None);

        // Assert
        Assert.AreEqual(2, attachmentCreatedIntegrationEvents.Count);
        await _syncToPCS4ServiceMock.Received(1)
            .SyncNewAttachmentAsync(attachmentCreatedIntegrationEvents.ElementAt(0), Arg.Any<CancellationToken>());
        await _syncToPCS4ServiceMock.Received(1)
            .SyncNewAttachmentAsync(attachmentCreatedIntegrationEvents.ElementAt(1), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotSyncNewAttachmentsWithPcs4_WhenPunchHasAttachments_AndNotCopy()
    {
        // Arrange
        SetupAttachmentDuplication(_commandWithoutCopyAttachments);

        // Act
        await _dut.Handle(_commandWithoutCopyAttachments, CancellationToken.None);

        // Assert
        await _syncToPCS4ServiceMock.DidNotReceive()
            .SyncNewAttachmentAsync(Arg.Any<AttachmentCreatedIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    #endregion

    private void FillAllProperties(PunchItem punchItemToDuplicate)
    {
        punchItemToDuplicate.SetActionBy(_existingPerson1);
        punchItemToDuplicate.SetClearingByOrg(_existingClearingByOrg1[TestPlantA]);
        punchItemToDuplicate.SetDocument(_existingDocument1[TestPlantA]);
        punchItemToDuplicate.SetOriginalWorkOrder(_existingWorkOrder1[TestPlantA]);
        punchItemToDuplicate.SetPriority(_existingPriority1[TestPlantA]);
        punchItemToDuplicate.SetRaisedByOrg(_existingRaisedByOrg1[TestPlantA]);
        punchItemToDuplicate.SetSWCR(_existingSWCR1[TestPlantA]);
        punchItemToDuplicate.SetSorting(_existingSorting1[TestPlantA]);
        punchItemToDuplicate.SetType(_existingType1[TestPlantA]);
        punchItemToDuplicate.SetWorkOrder(_existingWorkOrder2[TestPlantA]);
        punchItemToDuplicate.DueTimeUtc = DateTime.UtcNow;
        punchItemToDuplicate.Estimate = 2;
        punchItemToDuplicate.ExternalItemNo = "X";
        punchItemToDuplicate.MaterialETAUtc = DateTime.UtcNow;
        punchItemToDuplicate.MaterialExternalNo = "Y";
        punchItemToDuplicate.MaterialRequired = true;
    }

    private List<AttachmentCreatedIntegrationEvent> SetupAttachmentDuplication(DuplicatePunchItemCommand command)
    {
        var sourceAttachment = CreateAttachment(command.PunchItem.Guid);
        var attachments = new List<Attachment> { sourceAttachment };
        _attachmentRepositoryMock
            .GetAllByParentGuidAsync(command.PunchItem.Guid, Arg.Any<CancellationToken>())
            .Returns(attachments);

        var attachmentCreatedIntegrationEvent1 = new AttachmentCreatedIntegrationEvent(CreateAttachment(Guid.NewGuid()), TestPlantA);
        var attachmentCreatedIntegrationEvent2 = new AttachmentCreatedIntegrationEvent(CreateAttachment(Guid.NewGuid()), TestPlantA);
        List<AttachmentCreatedIntegrationEvent> attachmentCreatedIntegrationEvents1 = [attachmentCreatedIntegrationEvent1];
        List<AttachmentCreatedIntegrationEvent> attachmentCreatedIntegrationEvents2 = [attachmentCreatedIntegrationEvent2];
        
        _attachmentServiceMock.CopyAttachments(
                Arg.Is<List<Attachment>>(aList => aList.Count == 1 && aList.ElementAt(0).Guid == sourceAttachment.Guid),
                nameof(PunchItem),
                Arg.Any<Guid>(),
                command.PunchItem.Project.Name,
                Arg.Any<CancellationToken>())
            .Returns(
                attachmentCreatedIntegrationEvents1, 
                attachmentCreatedIntegrationEvents2 
                );

        return [attachmentCreatedIntegrationEvent1, attachmentCreatedIntegrationEvent2];
    }

    private Attachment CreateAttachment(Guid guid)
    {
        var sourceAttachment = new Attachment("P", nameof(PunchItem), guid, "fil");
        sourceAttachment.SetCreated(_currentPerson);
        return sourceAttachment;
    }
}
