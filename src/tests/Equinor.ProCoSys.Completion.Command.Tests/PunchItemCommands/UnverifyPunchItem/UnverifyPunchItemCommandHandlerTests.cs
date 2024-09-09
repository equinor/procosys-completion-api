using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnverifyPunchItem;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.UnverifyPunchItem;

[TestClass]
public class UnverifyPunchItemCommandHandlerTests : PunchItemCommandTestsBase
{
    private UnverifyPunchItemCommand _command;
    private UnverifyPunchItemCommandHandler _dut;

    [TestInitialize]
    public void Setup()
    {
        _command = new UnverifyPunchItemCommand(_existingPunchItem[TestPlantA].Guid, RowVersion)
        {
            PunchItem = _existingPunchItem[TestPlantA]
        };

        _command.PunchItem.Clear(_currentPerson);
        _command.PunchItem.Verify(_currentPerson);

        _dut = new UnverifyPunchItemCommandHandler(
            _syncToPCS4ServiceMock,
            _unitOfWorkMock,
            _messageProducerMock,
            Substitute.For<ILogger<UnverifyPunchItemCommandHandler>>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldUnverifyPunchItem()
    {
        // Assert
        Assert.IsTrue(_existingPunchItem[TestPlantA].VerifiedAtUtc.HasValue);
        Assert.IsTrue(_existingPunchItem[TestPlantA].VerifiedById.HasValue);

        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsFalse(_existingPunchItem[TestPlantA].VerifiedAtUtc.HasValue);
        Assert.IsFalse(_existingPunchItem[TestPlantA].VerifiedById.HasValue);
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
        Assert.AreEqual(_command.RowVersion, result);
        Assert.AreEqual(_command.RowVersion, _existingPunchItem[TestPlantA].RowVersion.ConvertToString());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldPublishPunchItemUpdatedIntegrationEvent()
    {
        // Arrange
        PunchItemUpdatedIntegrationEvent integrationEvent = null!;
        _messageProducerMock
            .When(x => x.PublishAsync(Arg.Any<PunchItemUpdatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                integrationEvent = callbackInfo.Arg<PunchItemUpdatedIntegrationEvent>();
            }));

        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsNotNull(integrationEvent);
        AssertNotVerified(integrationEvent);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSendHistoryUpdatedIntegrationEvent()
    {
        // Arrange
        HistoryUpdatedIntegrationEvent historyEvent = null!;
        _messageProducerMock
            .When(x => x.SendHistoryAsync(Arg.Any<HistoryUpdatedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                historyEvent = callbackInfo.Arg<HistoryUpdatedIntegrationEvent>();
            }));

        // Act
        await _dut.Handle(_command, default);

        // Assert
        var punchItem = _existingPunchItem[TestPlantA];
        AssertHistoryUpdatedIntegrationEvent(
            historyEvent,
            punchItem.Plant,
            "Punch item unverified",
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
        _messageProducerMock
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
        await _syncToPCS4ServiceMock.Received(1).SyncPunchListItemUpdateAsync(integrationEvent, default);
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
            await _dut.Handle(_command, default);
        });

        // Assert
        await _syncToPCS4ServiceMock.DidNotReceive().SyncPunchListItemUpdateAsync(Arg.Any<object>(), default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotThrowError_WhenSyncingWithPcs4Fails()
    {
        // Arrange
        _syncToPCS4ServiceMock.When(x => x.SyncPunchListItemUpdateAsync(Arg.Any<object>(), default))
            .Do(_ => throw new Exception("SyncPunchListItemUpdateAsync error"));

        // Act and Assert
        try
        {
            await _dut.Handle(_command, default);
        }
        catch (Exception ex)
        {
            Assert.Fail("Excepted no exception, but got: " + ex.Message);
        }
    }
    #endregion
}
