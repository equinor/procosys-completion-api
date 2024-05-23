using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItem;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.DeletePunchItem
{
    [TestClass]
    public class DeletePunchItemCommandHandlerTests : PunchItemCommandHandlerTestsBase
    {
        private readonly string _testPlant = TestPlantA;
        private DeletePunchItemCommand _command;
        private DeletePunchItemCommandHandler _dut;
        private ILogger<DeletePunchItemCommandHandler> _logger;

        [TestInitialize]
        public void Setup()
        {
            _command = new DeletePunchItemCommand(_existingPunchItem[_testPlant].Guid, RowVersion);

            _logger = Substitute.For<ILogger<DeletePunchItemCommandHandler>>();

            _dut = new DeletePunchItemCommandHandler(
                _punchItemRepositoryMock,
                _syncToPCS4ServiceMock,
                _unitOfWorkMock,
                _integrationEventPublisherMock,
                _logger);
        }

        [TestMethod]
        public async Task HandlingCommand_ShouldDeletePunchItemFromRepository()
        {
            // Act
            await _dut.Handle(_command, default);

            // Assert
            _punchItemRepositoryMock.Received(1).Remove(_existingPunchItem[_testPlant]);
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
            Assert.AreEqual(_command.RowVersion, _existingPunchItem[_testPlant].RowVersion.ConvertToString());
        }


        [TestMethod]
        public async Task HandlingCommand_ShouldPublishPunchItemDeletedIntegrationEvent()
        {
            // Arrange
            PunchItemDeletedIntegrationEvent integrationEvent = null!;
            _integrationEventPublisherMock
                .When(x => x.PublishAsync(Arg.Any<PunchItemDeletedIntegrationEvent>(), Arg.Any<CancellationToken>()))
                .Do(Callback.First(callbackInfo =>
                {
                    integrationEvent = callbackInfo.Arg<PunchItemDeletedIntegrationEvent>();
                }));

            // Act
            await _dut.Handle(_command, default);

            // Assert
            var punchItem = _existingPunchItem[_testPlant];
            Assert.AreEqual(punchItem.Plant, integrationEvent.Plant);
            Assert.AreEqual(punchItem.Guid, integrationEvent.Guid);
            Assert.AreEqual(punchItem.CheckListGuid, integrationEvent.ParentGuid);
            Assert.AreEqual(punchItem.ModifiedAtUtc, integrationEvent.DeletedAtUtc);
            Assert.AreEqual(punchItem.ModifiedBy!.Guid, integrationEvent.DeletedBy.Oid);
            Assert.AreEqual(punchItem.ModifiedBy!.GetFullName(), integrationEvent.DeletedBy.FullName);
        }

        [TestMethod]
        public async Task HandlingCommand_ShouldPublishHistoryDeletedIntegrationEvent()
        {
            // Arrange
            HistoryDeletedIntegrationEvent historyEvent = null!;
            _integrationEventPublisherMock
                .When(x => x.PublishAsync(Arg.Any<HistoryDeletedIntegrationEvent>(), Arg.Any<CancellationToken>()))
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
                $"Punch item {_existingPunchItem[_testPlant].Category} {_existingPunchItem[_testPlant].ItemNo} deleted",
                _existingPunchItem[_testPlant].CheckListGuid,
                _existingPunchItem[_testPlant],
                _existingPunchItem[_testPlant]);
        }

        #region Unit Tests which can be removed when no longer sync to pcs4
        [TestMethod]
        public async Task HandlingCommand_ShouldSyncWithPcs4()
        {
            // Arrange
            PunchItemDeletedIntegrationEvent integrationEvent = null!;

            _integrationEventPublisherMock
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
            //await _syncToPCS4ServiceMock.Received(1).SyncObjectDeletionAsync(SyncToPCS4Constants.PunchItem, integrationEvent, _testPlant, default);
            await _syncToPCS4ServiceMock.Received(1).SyncPunchListItemDeleteAsync(integrationEvent, default);
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
}
