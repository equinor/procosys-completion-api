﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnclearPunchItem;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Equinor.ProCoSys.Completion.DbSyncToPCS4;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.UnclearPunchItem;

[TestClass]
public class UnclearPunchItemCommandHandlerTests : PunchItemCommandHandlerTestsBase
{
    private readonly string _testPlant = TestPlantA;
    private UnclearPunchItemCommand _command;
    private UnclearPunchItemCommandHandler _dut;

    [TestInitialize]
    public void Setup()
    {
        _existingPunchItem[_testPlant].Clear(_currentPerson);

        _command = new UnclearPunchItemCommand(_existingPunchItem[_testPlant].Guid, RowVersion);

        _dut = new UnclearPunchItemCommandHandler(
            _punchItemRepositoryMock,
            _syncToPCS4ServiceMock,
            _unitOfWorkMock,
            _integrationEventPublisherMock,
            Substitute.For<ILogger<UnclearPunchItemCommandHandler>>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldUnclearPunchItem()
    {
        // Assert
        Assert.IsTrue(_existingPunchItem[_testPlant].ClearedAtUtc.HasValue);
        Assert.IsTrue(_existingPunchItem[_testPlant].ClearedById.HasValue);

        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsFalse(_existingPunchItem[_testPlant].ClearedAtUtc.HasValue);
        Assert.IsFalse(_existingPunchItem[_testPlant].ClearedById.HasValue);
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
        // Since UnitOfWorkMock is a Mock this will not happen here, so we assert that RowVersion is set from command
        Assert.AreEqual(_command.RowVersion, result.Data);
        Assert.AreEqual(_command.RowVersion, _existingPunchItem[_testPlant].RowVersion.ConvertToString());
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
        var punchItem = _existingPunchItem[_testPlant];
        Assert.IsNotNull(integrationEvent);
        AssertNotCleared(integrationEvent);
        AssertNotRejected(integrationEvent);
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
        var punchItem = _existingPunchItem[_testPlant];
        AssertHistoryUpdatedIntegrationEvent(
            historyEvent,
            punchItem.Plant,
            "Punch item uncleared",
            punchItem,
            punchItem);
        Assert.IsNotNull(historyEvent.ChangedProperties);
        Assert.AreEqual(0, historyEvent.ChangedProperties.Count);
    }

    #region Unit Tests which can be removed when no longer sync to pcs4
    [TestMethod]
    public async Task HandlingCommand_ShouldSyncWithPcs4()
    {
        // Arrange
        PunchItemUpdatedIntegrationEvent integrationEvent = null!;
        _integrationEventPublisherMock
            .When(x => x.PublishAsync(
                Arg.Any<PunchItemUpdatedIntegrationEvent>(),
                default))
            .Do(info =>
            {
                integrationEvent = info.Arg<PunchItemUpdatedIntegrationEvent>();
            });

        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _syncToPCS4ServiceMock.Received(1).SyncObjectUpdateAsync(SyncToPCS4Constants.PunchItem, integrationEvent, _testPlant, default);
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
