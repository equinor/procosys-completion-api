using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItem;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.DeletePunchItem;

[TestClass]
public class DeletePunchItemCommandHandlerTests : PunchItemCommandHandlerTestsBase
{
    private DeletePunchItemCommand _command;
    private DeletePunchItemCommandHandler _dut;

    [TestInitialize]
    public void Setup()
    {
        _command = new DeletePunchItemCommand(_existingPunchItem.Guid, _rowVersion);

        _dut = new DeletePunchItemCommandHandler(
            _punchItemRepositoryMock.Object,
            _unitOfWorkMock.Object,
            new Mock<ILogger<DeletePunchItemCommandHandler>>().Object);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldDeletePunchItemFromRepository()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        _punchItemRepositoryMock.Verify(r => r.Remove(_existingPunchItem), Times.Once);
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
        Assert.AreEqual(_rowVersion, _existingPunchItem.RowVersion.ConvertToString());
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldAddPunchItemDeletedEvent()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsInstanceOfType(_existingPunchItem.DomainEvents.Last(), typeof(PunchItemDeletedDomainEvent));
    }
}
