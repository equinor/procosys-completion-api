using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.DeletePunchItem;

[TestClass]
public class DeletePunchItemCommandHandlerTests : PunchItemCommandTestsBase
{
    private DeletePunchItemCommand _command;
    private DeletePunchItemCommandHandler _dut;

    [TestInitialize]
    public void Setup()
    {
        _command = new DeletePunchItemCommand(_existingPunchItem[TestPlantA].Guid, RowVersion)
        {
            PunchItem = _existingPunchItem[TestPlantA]
        };

        _dut = new DeletePunchItemCommandHandler(
            _punchItemRepositoryMock,
            _syncToPCS4ServiceMock,
            _unitOfWorkMock,
            _messageProducerMock,
            _checkListApiServiceMock,
            Substitute.For<ILogger<DeletePunchItemCommandHandler>>(),
            Substitute.For<ICommentRepository>(),
            Substitute.For<IAttachmentRepository>(),
            Substitute.For<ILinkRepository>());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldDeletePunchItemFromRepository()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        _punchItemRepositoryMock.Received(1).Remove(_existingPunchItem[TestPlantA]);
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
        await _dut.Handle(_command, default);

        // Assert
        // In real life EF Core will create a new RowVersion when save.
        // Since UnitOfWorkMock is a Mock this will not happen here, so we assert that RowVersion is set from command
        Assert.AreEqual(_command.RowVersion, _existingPunchItem[TestPlantA].RowVersion.ConvertToString());
    }


    [TestMethod]
    public async Task HandlingCommand_ShouldPublishPunchItemDeletedIntegrationEvent()
    {
        // Arrange
        PunchItemDeletedIntegrationEvent integrationEvent = null!;
        _messageProducerMock
            .When(x => x.PublishAsync(Arg.Any<PunchItemDeletedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                integrationEvent = callbackInfo.Arg<PunchItemDeletedIntegrationEvent>();
            }));

        // Act
        await _dut.Handle(_command, default);

        // Assert
        var punchItem = _existingPunchItem[TestPlantA];
        Assert.AreEqual(punchItem.Plant, integrationEvent.Plant);
        Assert.AreEqual(punchItem.Guid, integrationEvent.Guid);
        Assert.AreEqual(punchItem.CheckListGuid, integrationEvent.ParentGuid);
        Assert.AreEqual(punchItem.ModifiedAtUtc, integrationEvent.DeletedAtUtc);
        Assert.AreEqual(punchItem.ModifiedBy!.Guid, integrationEvent.DeletedBy.Oid);
        Assert.AreEqual(punchItem.ModifiedBy!.GetFullName(), integrationEvent.DeletedBy.FullName);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSendHistoryDeletedIntegrationEvent()
    {
        // Arrange
        HistoryDeletedIntegrationEvent historyEvent = null!;
        _messageProducerMock
            .When(x => x.SendHistoryAsync(Arg.Any<HistoryDeletedIntegrationEvent>(), Arg.Any<CancellationToken>()))
            .Do(Callback.First(callbackInfo =>
            {
                historyEvent = callbackInfo.Arg<HistoryDeletedIntegrationEvent>();
            }));

        // Act
        await _dut.Handle(_command, default);

        // Assert
        AssertHistoryDeletedIntegrationEvent(
            historyEvent,
            _plantProviderMock.Plant,
            $"Punch item {_existingPunchItem[TestPlantA].Category} {_existingPunchItem[TestPlantA].ItemNo} deleted",
            _existingPunchItem[TestPlantA].CheckListGuid,
            _existingPunchItem[TestPlantA],
            _existingPunchItem[TestPlantA]);
    }

    #region Unit Tests which can be removed when no longer sync to pcs4
    [TestMethod]
    public async Task HandlingCommand_ShouldRecalculateChecklist()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        var punchItem = _existingPunchItem[TestPlantA];
        await _checkListApiServiceMock.Received(1).RecalculateCheckListStatusAsync(TestPlantA, punchItem.CheckListGuid, default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSyncWithPcs4()
    {
        // Arrange
        PunchItemDeletedIntegrationEvent integrationEvent = null!;

        _messageProducerMock
            .When(x => x.PublishAsync(
                Arg.Any<PunchItemDeletedIntegrationEvent>(),
                default))
            .Do(info =>
            {
                integrationEvent = info.Arg<PunchItemDeletedIntegrationEvent>();
            });

        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _syncToPCS4ServiceMock.Received(1).SyncPunchListItemDeleteAsync(integrationEvent, default);
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
        await _syncToPCS4ServiceMock.DidNotReceive().SyncPunchListItemDeleteAsync(Arg.Any<object>(), default);
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
            await _dut.Handle(_command, default);
        });

        // Assert
        await _checkListApiServiceMock.DidNotReceive().RecalculateCheckListStatusAsync(Arg.Any<string>(), Arg.Any<Guid>(), default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotRecalculate_WhenSyncingWithPcs4Fails()
    {
        // Arrange
        _syncToPCS4ServiceMock.When(x => x.SyncPunchListItemDeleteAsync(Arg.Any<object>(), default))
            .Do(_ => throw new Exception("SyncPunchListItemDeleteAsync error"));

        // Act
        await _dut.Handle(_command, default);

        // Assert
        await _checkListApiServiceMock.DidNotReceive().RecalculateCheckListStatusAsync(Arg.Any<string>(), Arg.Any<Guid>(), default);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldNotThrowError_WhenSyncingWithPcs4Fails()
    {
        // Arrange
        _syncToPCS4ServiceMock.When(x => x.SyncPunchListItemDeleteAsync(Arg.Any<object>(), default))
            .Do(_ => throw new Exception("SyncPunchListItemDeleteAsync error"));

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
        _checkListApiServiceMock.When(x => x.RecalculateCheckListStatusAsync(Arg.Any<string>(), Arg.Any<Guid>(), default))
            .Do(_ => throw new Exception("RecalculateCheckListStatus error"));

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
