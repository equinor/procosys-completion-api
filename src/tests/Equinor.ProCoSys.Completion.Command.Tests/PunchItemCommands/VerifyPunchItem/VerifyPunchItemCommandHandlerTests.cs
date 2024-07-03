using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.VerifyPunchItem;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.VerifyPunchItem;

[TestClass]
public class VerifyPunchItemCommandHandlerTests : PunchItemCommandHandlerTestsBase
{
    private readonly string _testPlant = TestPlantA;
    private VerifyPunchItemCommand _command;
    private VerifyPunchItemCommandHandler _dut;

    [TestInitialize]
    public void Setup()
    {
        _existingPunchItem[_testPlant].Clear(_currentPerson);

        _command = new VerifyPunchItemCommand(_existingPunchItem[_testPlant].Guid, RowVersion);

        _dut = new VerifyPunchItemCommandHandler(
            _punchItemRepositoryMock,
            _personRepositoryMock,
            _syncToPCS4ServiceMock,
            _unitOfWorkMock,
            _messageProducerMock,
             Substitute.For<ILogger<VerifyPunchItemCommandHandler>>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldVerifyPunchItem()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.AreEqual(_utcNow, _existingPunchItem[_testPlant].VerifiedAtUtc);
        Assert.AreEqual(_currentPerson.Id, _existingPunchItem[_testPlant].VerifiedById);
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
        _messageProducerMock
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
        AssertIsVerified(punchItem, punchItem.VerifiedBy, integrationEvent);
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
        var punchItem = _existingPunchItem[_testPlant];
        AssertHistoryUpdatedIntegrationEvent(
            historyEvent,
            punchItem.Plant,
            "Punch item verified",
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
