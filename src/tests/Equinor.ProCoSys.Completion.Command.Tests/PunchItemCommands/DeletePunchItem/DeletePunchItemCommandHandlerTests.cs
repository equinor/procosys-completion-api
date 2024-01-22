using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.PunchItem;
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
            await _syncToPCS4ServiceMock.Received(1).SyncObjectDeletionAsync("PunchItem", integrationEvent, _testPlant, default);
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
            var punchItem = _existingPunchItem[_testPlant];
            Assert.IsNotNull(integrationEvent);
            AssertRequiredProperties(punchItem, integrationEvent);
            AssertOptionalProperties(punchItem, integrationEvent);
            AssertIsVerified(punchItem, punchItem.VerifiedBy, integrationEvent);
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
    }
}
