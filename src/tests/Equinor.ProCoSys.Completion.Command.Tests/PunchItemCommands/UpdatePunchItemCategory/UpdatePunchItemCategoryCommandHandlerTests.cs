using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemCategory;
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
            _messageProducerMock,
            _checkListApiServiceMock,
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
        _messageProducerMock
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
        Assert.AreEqual(Category.PB.ToString(), changedProperty.Value);
    }

    #region Unit Tests which can be removed when no longer sync to pcs4
    [TestMethod]
    public async Task HandlingCommand_ShouldRecalculateChecklist()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        var punchItem = await _punchItemRepositoryMock.GetAsync(_command.PunchItemGuid, default);
        await _checkListApiServiceMock.Received(1).RecalculateCheckListStatus(_testPlant, punchItem.CheckListGuid, default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSyncWithPcs4()
    {
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
        await _syncToPCS4ServiceMock.Received(1).SyncPunchListItemUpdateAsync(integrationEvent, default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotSyncWithPcs4_WhenSavingChangesFails()
    {
        // Arrange
        _unitOfWorkMock.When(x => x.SaveChangesAsync(default))
            .Do(x => throw new Exception("SaveChangesAsync error"));

        // Act
        await Assert.ThrowsExceptionAsync<Exception>(async () =>
        {
            await _dut.Handle(_command, default);
        });

        // Assert
        await _syncToPCS4ServiceMock.DidNotReceive().SyncPunchListItemUpdateAsync(Arg.Any<object>(), default);
        _unitOfWorkMock.ClearReceivedCalls();
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldRecalculate()
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
        await _checkListApiServiceMock.Received(1).RecalculateCheckListStatus(Arg.Any<string>(), Arg.Any<Guid>(), default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotRecalculate_WhenSavingChangesFails()
    {
        // Arrange
        _unitOfWorkMock.When(x => x.SaveChangesAsync(default))
            .Do(x => throw new Exception("SaveChangesAsync error"));

        // Act
        await Assert.ThrowsExceptionAsync<Exception>(async () =>
        {
            await _dut.Handle(_command, default);
        });

        // Assert
        await _checkListApiServiceMock.DidNotReceive().RecalculateCheckListStatus(Arg.Any<string>(), Arg.Any<Guid>(), default);
        _unitOfWorkMock.ClearReceivedCalls();
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotRecalculate_WhenSyncingWithPcs4Fails()
    {
        // Arrange
        _syncToPCS4ServiceMock.When(x => x.SyncPunchListItemUpdateAsync(Arg.Any<object>(), default))
            .Do(x => throw new Exception("SyncPunchListItemUpdateAsync error"));

        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _checkListApiServiceMock.DidNotReceive().RecalculateCheckListStatus(Arg.Any<string>(), Arg.Any<Guid>(), default);
        _unitOfWorkMock.ClearReceivedCalls();
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotThrowError_WhenSyncingWithPcs4Fails()
    {
        // Arrange
        _syncToPCS4ServiceMock.When(x => x.SyncPunchListItemUpdateAsync(Arg.Any<object>(), default))
            .Do(x => throw new Exception("SyncPunchListItemUpdateAsync error"));

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

    [TestMethod]
    public async Task HandlingCommand_ShouldNotThrowError_WhenRecalculatingFails()
    {
        // Arrange
        _checkListApiServiceMock.When(x => x.RecalculateCheckListStatus(Arg.Any<string>(), Arg.Any<Guid>(), default))
            .Do(x => throw new Exception("RecalculateCheckListStatus error"));

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
