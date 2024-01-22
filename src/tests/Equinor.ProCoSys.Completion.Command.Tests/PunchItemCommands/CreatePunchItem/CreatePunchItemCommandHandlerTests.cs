using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Equinor.ProCoSys.Completion.DbSyncToPCS4;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using User = Equinor.ProCoSys.Completion.MessageContracts.User;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.CreatePunchItem;

[TestClass]
public class CreatePunchItemCommandHandlerTests : PunchItemCommandHandlerTestsBase
{
    private readonly string _testPlant = TestPlantA;
    private readonly Guid _existingCheckListGuid = Guid.NewGuid();
    private PunchItem _punchItemAddedToRepository;
    private readonly int _punchItemId = 17;

    private CreatePunchItemCommandHandler _dut;
    private CreatePunchItemCommand _command;

    [TestInitialize]
    public void Setup()
    {
        _punchItemRepositoryMock
            .When(x => x.Add(Arg.Any<PunchItem>()))
            .Do(callInfo =>
            {
                _punchItemAddedToRepository = callInfo.Arg<PunchItem>();
            });

        _unitOfWorkMock
            .When(x => x.SaveChangesAsync())
            .Do(Callback.First(_ =>
            {
                _punchItemAddedToRepository.SetCreated(_currentPerson);
                _punchItemAddedToRepository.SetProtectedIdForTesting(_punchItemId);
            }));

        _command = new CreatePunchItemCommand(
            Category.PA,
            "P123",
            _existingProject[_testPlant].Guid,
            _existingCheckListGuid,
            _existingRaisedByOrg1[_testPlant].Guid,
            _existingClearingByOrg1[_testPlant].Guid,
            _existingPerson1.Guid,
            DateTime.UtcNow,
            _existingPriority1[_testPlant].Guid,
            _existingSorting1[_testPlant].Guid,
            _existingType1[_testPlant].Guid,
            100,
            _existingWorkOrder1[_testPlant].Guid,
            _existingWorkOrder2[_testPlant].Guid,
            _existingSWCR1[_testPlant].Guid,
            _existingDocument1[_testPlant].Guid,
            "123",
            true,
            DateTime.UtcNow,
            "123.1");

        _dut = new CreatePunchItemCommandHandler(
            _plantProviderMock,
            _punchItemRepositoryMock,
            _libraryItemRepositoryMock,
            _projectRepositoryMock,
            _personRepositoryMock,
            _workOrderRepositoryMock,
            _swcrRepositoryMock,
            _documentRepositoryMock,
            _syncToPCS4ServiceMock,
            _unitOfWorkMock,
            _integrationEventPublisherMock,
            Substitute.For<ILogger<CreatePunchItemCommandHandler>>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldReturn_GuidAndRowVersion()
    {
        // Act
        var result = await _dut.Handle(_command, default);

        // Assert
        Assert.IsInstanceOfType(result.Data, typeof(GuidAndRowVersion));
    }

    [TestMethod]
    public async Task HandlingCommand_WithAllValues_ShouldAddCorrectPunchItem_ToPunchItemRepository()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsNotNull(_punchItemAddedToRepository);
        Assert.AreEqual(_command.Description, _punchItemAddedToRepository.Description);
        Assert.AreEqual(_existingProject[_testPlant].Id, _punchItemAddedToRepository.ProjectId);
        Assert.AreEqual(_existingRaisedByOrg1[_testPlant].Id, _punchItemAddedToRepository.RaisedByOrgId);
        Assert.AreEqual(_existingClearingByOrg1[_testPlant].Id, _punchItemAddedToRepository.ClearingByOrgId);
        Assert.AreEqual(_existingPerson1.Id, _punchItemAddedToRepository.ActionById);
        Assert.AreEqual(_command.DueTimeUtc, _punchItemAddedToRepository.DueTimeUtc);
        Assert.AreEqual(_existingPriority1[_testPlant].Id, _punchItemAddedToRepository.PriorityId);
        Assert.AreEqual(_existingSorting1[_testPlant].Id, _punchItemAddedToRepository.SortingId);
        Assert.AreEqual(_existingType1[_testPlant].Id, _punchItemAddedToRepository.TypeId);
        Assert.AreEqual(_command.Estimate, _punchItemAddedToRepository.Estimate);
        Assert.AreEqual(_existingWorkOrder1[_testPlant].Id, _punchItemAddedToRepository.OriginalWorkOrderId);
        Assert.AreEqual(_existingWorkOrder2[_testPlant].Id, _punchItemAddedToRepository.WorkOrderId);
        Assert.AreEqual(_existingSWCR1[_testPlant].Id, _punchItemAddedToRepository.SWCRId);
        Assert.AreEqual(_existingDocument1[_testPlant].Id, _punchItemAddedToRepository.DocumentId);
        Assert.AreEqual(_command.ExternalItemNo, _punchItemAddedToRepository.ExternalItemNo);
        Assert.AreEqual(_command.MaterialRequired, _punchItemAddedToRepository.MaterialRequired);
        Assert.AreEqual(_command.MaterialETAUtc, _punchItemAddedToRepository.MaterialETAUtc);
        Assert.AreEqual(_command.MaterialExternalNo, _punchItemAddedToRepository.MaterialExternalNo);
    }

    [TestMethod]
    public async Task HandlingCommand_WithRequiredValues_ShouldAddCorrectPunchItem_ToPunchItemRepository()
    {
        // Arrange
        var command = new CreatePunchItemCommand(
            Category.PA,
            "P123",
            _existingProject[_testPlant].Guid,
            _existingCheckListGuid,
            _existingRaisedByOrg1[_testPlant].Guid,
            _existingClearingByOrg1[_testPlant].Guid,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            false,
            null,
            null);

        // Act
        await _dut.Handle(command, default);

        // Assert
        Assert.IsNotNull(_punchItemAddedToRepository);
        Assert.AreEqual(_command.Description, _punchItemAddedToRepository.Description);
        Assert.AreEqual(_existingProject[_testPlant].Id, _punchItemAddedToRepository.ProjectId);
        Assert.AreEqual(_existingRaisedByOrg1[_testPlant].Id, _punchItemAddedToRepository.RaisedByOrgId);
        Assert.AreEqual(_existingClearingByOrg1[_testPlant].Id, _punchItemAddedToRepository.ClearingByOrgId);
        Assert.IsFalse(_punchItemAddedToRepository.ActionById.HasValue);
        Assert.IsFalse(_punchItemAddedToRepository.DueTimeUtc.HasValue);
        Assert.IsFalse(_punchItemAddedToRepository.PriorityId.HasValue);
        Assert.IsFalse(_punchItemAddedToRepository.SortingId.HasValue);
        Assert.IsFalse(_punchItemAddedToRepository.TypeId.HasValue);
        Assert.IsFalse(_punchItemAddedToRepository.Estimate.HasValue);
        Assert.IsFalse(_punchItemAddedToRepository.OriginalWorkOrderId.HasValue);
        Assert.IsFalse(_punchItemAddedToRepository.WorkOrderId.HasValue);
        Assert.IsFalse(_punchItemAddedToRepository.SWCRId.HasValue);
        Assert.IsFalse(_punchItemAddedToRepository.DocumentId.HasValue);
        Assert.IsNull(_punchItemAddedToRepository.ExternalItemNo);
        Assert.IsFalse(_punchItemAddedToRepository.MaterialRequired);
        Assert.IsFalse(_punchItemAddedToRepository.MaterialETAUtc.HasValue);
        Assert.IsNull(_punchItemAddedToRepository.MaterialExternalNo);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSaveTwice()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _unitOfWorkMock.Received(2).SaveChangesAsync();
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldPublishPunchItemCreatedIntegrationEvent()
    {
        // Arrange
        PunchItemCreatedIntegrationEvent integrationEvent = null!;
        _integrationEventPublisherMock
            .When(x => x.PublishAsync(Arg.Any<PunchItemCreatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                integrationEvent = callbackInfo.Arg<PunchItemCreatedIntegrationEvent>();
            }));

        // Act
        await _dut.Handle(_command, default);

        // Assert
        var punchItem = _punchItemAddedToRepository;
        Assert.IsNotNull(integrationEvent);
        AssertRequiredProperties(punchItem, integrationEvent);
        AssertOptionalProperties(punchItem, integrationEvent);
        AssertNotCleared(integrationEvent);
        AssertNotRejected(integrationEvent);
        AssertNotVerified(integrationEvent);
    }

    [TestMethod]
    public async Task HandlingCommand_WithAllPropertiesSet_ShouldPublishHistoryCreatedIntegrationEvent()
    {
        // Arrange
        HistoryCreatedIntegrationEvent historyEvent = null!;
        _integrationEventPublisherMock
            .When(x => x.PublishAsync(Arg.Any<HistoryCreatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                historyEvent = callbackInfo.Arg<HistoryCreatedIntegrationEvent>();
            }));
        // Act
        await _dut.Handle(_command, default);

        // Assert
        AssertHistoryCreatedIntegrationEvent(
            historyEvent,
            _punchItemAddedToRepository.Plant,
            $"Punch item {_punchItemAddedToRepository.Category} {_punchItemAddedToRepository.ItemNo} created",
            _punchItemAddedToRepository.CheckListGuid,
            _punchItemAddedToRepository,
            _punchItemAddedToRepository);
       
        var properties = historyEvent.Properties;
        Assert.IsNotNull(properties);
        Assert.AreEqual(19, properties.Count);
        AssertProperty(
            properties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.ItemNo)),
            _punchItemAddedToRepository.ItemNo);
        AssertProperty(
            properties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Category)),
            _punchItemAddedToRepository.Category.ToString());
        AssertProperty(
            properties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Description)),
            _punchItemAddedToRepository.Description);
        AssertProperty(
            properties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.RaisedByOrg)),
            _punchItemAddedToRepository.RaisedByOrg.Code);
        AssertProperty(
            properties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.ClearingByOrg)),
            _punchItemAddedToRepository.ClearingByOrg.Code);
        AssertPerson(
            properties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.ActionBy)),
            new User(_existingPerson1.Guid, _existingPerson1.GetFullName()));
        AssertProperty(
            properties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.DueTimeUtc)),
            _punchItemAddedToRepository.DueTimeUtc);
        AssertProperty(
            properties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Priority)),
            _punchItemAddedToRepository.Priority!.Code);
        AssertProperty(
            properties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Sorting)),
            _punchItemAddedToRepository.Sorting!.Code);
        AssertProperty(
            properties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Type)),
            _punchItemAddedToRepository.Type!.Code);
        AssertProperty(
            properties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Estimate)),
            _punchItemAddedToRepository.Estimate!.Value);

        AssertProperty(
            properties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Document)),
            _punchItemAddedToRepository.Document!.No);
        AssertProperty(
            properties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.SWCR)),
            _punchItemAddedToRepository.SWCR!.No);
        AssertProperty(
            properties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.OriginalWorkOrder)),
            _punchItemAddedToRepository.OriginalWorkOrder!.No);
        AssertProperty(
            properties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.WorkOrder)),
            _punchItemAddedToRepository.WorkOrder!.No);

        AssertProperty(
            properties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.ExternalItemNo)),
            _punchItemAddedToRepository.ExternalItemNo);
        AssertProperty(
            properties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.MaterialRequired)),
            _punchItemAddedToRepository.MaterialRequired);
        AssertProperty(
            properties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.MaterialETAUtc)),
            _punchItemAddedToRepository.MaterialETAUtc!.Value);
        AssertProperty(
            properties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.MaterialExternalNo)),
            _punchItemAddedToRepository.MaterialExternalNo);
    }

    [TestMethod]
    public async Task HandlingCommand_WithOnlyRequiredPropertiesSet_ShouldPublishCorrectHistoryEvent()
    {
        // Arrange
        HistoryCreatedIntegrationEvent historyEvent = null!;
        _integrationEventPublisherMock
            .When(x => x.PublishAsync(Arg.Any<HistoryCreatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                historyEvent = callbackInfo.Arg<HistoryCreatedIntegrationEvent>();
            }));

        // Arrange
        var command = new CreatePunchItemCommand(
            Category.PA,
            "P123",
            _existingProject[_testPlant].Guid,
            _existingCheckListGuid,
            _existingRaisedByOrg1[_testPlant].Guid,
            _existingClearingByOrg1[_testPlant].Guid,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            false,
            null,
            null);

        // Act
        await _dut.Handle(command, default);

        // Assert
        AssertHistoryCreatedIntegrationEvent(
            historyEvent,
            _punchItemAddedToRepository.Plant,
            $"Punch item {_punchItemAddedToRepository.Category} {_punchItemAddedToRepository.ItemNo} created",
            _punchItemAddedToRepository.CheckListGuid,
            _punchItemAddedToRepository,
            _punchItemAddedToRepository);
        var properties = historyEvent.Properties;

        Assert.IsNotNull(properties);
        Assert.AreEqual(5, properties.Count);
        AssertProperty(
            properties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.ItemNo)),
            _punchItemAddedToRepository.ItemNo);
        AssertProperty(
            properties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Category)),
            _punchItemAddedToRepository.Category.ToString());
        AssertProperty(
            properties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.Description)),
            _punchItemAddedToRepository.Description);
        AssertProperty(
            properties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.RaisedByOrg)),
            _punchItemAddedToRepository.RaisedByOrg.Code);
        AssertProperty(
            properties
                .SingleOrDefault(c => c.Name == nameof(PunchItem.ClearingByOrg)),
            _punchItemAddedToRepository.ClearingByOrg.Code);
    }

    #region Unit Tests which can be removed when no longer sync to pcs4
    [TestMethod]
    public async Task HandlingCommand_ShouldSyncWithPcs4()
    {
        // Arrange
        PunchItemCreatedIntegrationEvent integrationEvent = null!;
        _integrationEventPublisherMock
            .When(x => x.PublishAsync(
                Arg.Any<PunchItemCreatedIntegrationEvent>(),
                default))
            .Do(info =>
            {
                integrationEvent = info.Arg<PunchItemCreatedIntegrationEvent>();
            });


        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _syncToPCS4ServiceMock.Received(1).SyncNewObjectAsync(SyncToPCS4Service.PunchItem, integrationEvent, _testPlant, default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldBeginTransaction()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _unitOfWorkMock.Received(1).BeginTransactionAsync(default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldCommitTransaction_WhenNoExceptions()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _unitOfWorkMock.Received(1).CommitTransactionAsync(default);
        await _unitOfWorkMock.Received(0).RollbackTransactionAsync(default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldRollbackTransaction_WhenExceptionThrown()
    {
        // Arrange
        _unitOfWorkMock
            .When(u => u.SaveChangesAsync())
            .Do(_ => throw new Exception());

        // Act
        var exception = await Assert.ThrowsExceptionAsync<Exception>(() => _dut.Handle(_command, default));

        // Assert
        await _unitOfWorkMock.Received(0).CommitTransactionAsync(default);
        await _unitOfWorkMock.Received(1).RollbackTransactionAsync(default);
        Assert.IsInstanceOfType(exception, typeof(Exception));
    }
    #endregion
}
