using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItem;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
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
                _unitOfWorkMock,
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
        public async Task HandlingCommand_ShouldSave()
        {
            // Act
            await _dut.Handle(_command, default);

            // Assert
            await _unitOfWorkMock.Received(1).SaveChangesAsync(default);
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
        public async Task HandlingCommand_ShouldAddPunchItemDeletedEvent()
        {
            // Act
            await _dut.Handle(_command, default);

            // Assert
            Assert.IsInstanceOfType(_existingPunchItem[_testPlant].DomainEvents.Last(), typeof(PunchItemDeletedDomainEvent));
        }
    }
}
