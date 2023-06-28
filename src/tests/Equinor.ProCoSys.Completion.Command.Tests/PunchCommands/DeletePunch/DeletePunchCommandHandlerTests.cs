using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchCommands.DeletePunch;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchCommands.DeletePunch;

[TestClass]
public class DeletePunchCommandHandlerTests : TestsBase
{
    private readonly string _rowVersion = "AAAAAAAAABA=";

    private Mock<IPunchRepository> _punchRepositoryMock;
    private Punch _existingPunch;

    private DeletePunchCommand _command;
    private DeletePunchCommandHandler _dut;

    [TestInitialize]
    public void Setup()
    {
        var project = new Project(TestPlantA, Guid.NewGuid(), "P", "D");
        _existingPunch = new Punch(TestPlantA, project, "Punch");
        _punchRepositoryMock = new Mock<IPunchRepository>();
        _punchRepositoryMock.Setup(r => r.GetByGuidAsync(_existingPunch.Guid))
            .ReturnsAsync(_existingPunch);

        _command = new DeletePunchCommand(_existingPunch.Guid, _rowVersion);

        _dut = new DeletePunchCommandHandler(
            _punchRepositoryMock.Object,
            _unitOfWorkMock.Object,
            new Mock<ILogger<DeletePunchCommandHandler>>().Object);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldDeletePunchFromRepository()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        _punchRepositoryMock.Verify(r => r.Remove(_existingPunch), Times.Once);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSave()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSetAndReturnRowVersion()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        // In real life EF Core will create a new RowVersion when save.
        // Since UnitOfWorkMock is a Mock this will not happen here, so we assert that RowVersion is set from command
        Assert.AreEqual(_rowVersion, _existingPunch.RowVersion.ConvertToString());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldAddPunchDeletedEvent()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsInstanceOfType(_existingPunch.DomainEvents.Last(), typeof(PunchDeletedEvent));
    }
}
