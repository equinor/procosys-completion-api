using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemCategory;
using Equinor.ProCoSys.Completion.DbSyncToPCS4;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.UpdatePunchItemCategory;

[TestClass]
public class UpdatePunchItemCategoryCommandHandlerTests : PunchItemCommandHandlerTestsBase
{
    private readonly string _testPlant = TestPlantA;
    private UpdatePunchItemCategoryCommand _command;
    private UpdatePunchItemCategoryCommandHandler _dut;

    [TestInitialize]
    public void Setup()
    {
        _command = new UpdatePunchItemCategoryCommand(_punchItemPa[_testPlant].Guid, Category.PB, RowVersion);

        _dut = new UpdatePunchItemCategoryCommandHandler(
            _punchItemRepositoryMock,
            _syncToPCS4ServiceMock,
            _unitOfWorkMock,
            _integrationEventPublisherMock,
            Substitute.For<ILogger<UpdatePunchItemCategoryCommandHandler>>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldChangePaPunchItemToPb()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(Category.PB, _punchItemPa[_testPlant].Category);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSetAuditData()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _unitOfWorkMock.Received(1).SetAuditDataAsync();
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSave()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync();
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSetAndReturnRowVersion()
    {
        // Act
        var result = await _dut.Handle(_command, default);

        // Assert
        // In real life EF Core will create a new RowVersion when save.
        // Since UnitOfWorkMock is a Substitute this will not happen here, so we assert that RowVersion is set from command
        Assert.AreEqual(_command.RowVersion, result.Data);
        Assert.AreEqual(_command.RowVersion, _punchItemPa[_testPlant].RowVersion.ConvertToString());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldPublishPunchItemUpdatedIntegrationEvent()
    {
        // Arrange
        PunchItemUpdatedIntegrationEvent integrationEvent = null!;
        _integrationEventPublisherMock
            .When(x => x.PublishAsync(Arg.Any<PunchItemUpdatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                integrationEvent = callbackInfo.Arg<PunchItemUpdatedIntegrationEvent>();
            }));

        // Act
        await _dut.Handle(_command, default);

        // Assert
        var punchItem = _punchItemPa[_testPlant];
        Assert.IsNotNull(integrationEvent);
        Assert.AreEqual(punchItem.Category.ToString(), integrationEvent.Category);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldPublishHistoryUpdatedIntegrationEvent()
    {
        // Arrange
        HistoryUpdatedIntegrationEvent historyEvent = null!;
        _integrationEventPublisherMock
            .When(x => x.PublishAsync(Arg.Any<HistoryUpdatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                historyEvent = callbackInfo.Arg<HistoryUpdatedIntegrationEvent>();
            }));

        // Act
        await _dut.Handle(_command, default);

        // Assert
        var punchItem = _punchItemPa[_testPlant];
        AssertHistoryUpdatedIntegrationEvent(
            historyEvent,
            punchItem.Plant,
            $"Punch item category changed to {_command.Category}",
            punchItem,
            punchItem);
        var changedProperties = historyEvent.ChangedProperties;
        Assert.IsNotNull(changedProperties);
        Assert.AreEqual(1, changedProperties.Count);
        var changedProperty = changedProperties[0];
        Assert.AreEqual(nameof(PunchItem.Category), changedProperty.Name);
        Assert.AreEqual(Category.PA.ToString(), changedProperty.OldValue);
        Assert.AreEqual(Category.PB.ToString(), changedProperty.NewValue);
    }

    #region Unit Tests which can be removed when no longer sync to pcs4

    [TestMethod]
    public async Task HandlingCommand_ShouldSyncWithPcs4()
    {
        PunchItemUpdatedIntegrationEvent integrationEvent = null!;
        _integrationEventPublisherMock
            .When(x => x.PublishAsync(Arg.Any<PunchItemUpdatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                integrationEvent = callbackInfo.Arg<PunchItemUpdatedIntegrationEvent>();
            }));

        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _syncToPCS4ServiceMock.Received(1).SyncObjectUpdateAsync(SyncToPCS4Service.PunchItem, integrationEvent, _testPlant, default);
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
