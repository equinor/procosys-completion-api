﻿using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.ClearPunchItem;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.ClearPunchItem
{
    [TestClass]
    public class ClearPunchItemCommandHandlerTests : PunchItemCommandHandlerTestsBase
    {
        private readonly string _testPlant = TestPlantA;
        private ClearPunchItemCommand _command;
        private ClearPunchItemCommandHandler _dut;

        [TestInitialize]
        public void Setup()
        {
            _command = new ClearPunchItemCommand(_existingPunchItem[_testPlant].Guid, RowVersion);

            _dut = new ClearPunchItemCommandHandler(
                _punchItemRepositoryMock,
                _personRepositoryMock,
                _unitOfWorkMock,
                Substitute.For<ILogger<ClearPunchItemCommandHandler>>());
        }

        [TestMethod]
        public async Task HandlingCommand_ShouldClearPunchItem()
        {
            // Act
            await _dut.Handle(_command, default);

            // Assert
            Assert.AreEqual(_utcNow, _existingPunchItem[_testPlant].ClearedAtUtc);
            Assert.AreEqual(_currentPerson.Id, _existingPunchItem[_testPlant].ClearedById);
        }

        [TestMethod]
        public async Task HandlingCommand_ShouldSave()
        {
            // Act
            await _dut.Handle(_command, default);

            // Assert
          await  _unitOfWorkMock.Received(1).SaveChangesAsync(default);
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
            Assert.AreEqual(_command.RowVersion, _existingPunchItem[_testPlant].RowVersion.ConvertToString());
        }

        [TestMethod]
        public async Task HandlingCommand_ShouldAddPunchItemClearedDomainEvent()
        {
            // Act
            await _dut.Handle(_command, default);

            // Assert
            Assert.IsInstanceOfType(_existingPunchItem[_testPlant].DomainEvents.Last(), typeof(PunchItemClearedDomainEvent));
        }
    }
}
