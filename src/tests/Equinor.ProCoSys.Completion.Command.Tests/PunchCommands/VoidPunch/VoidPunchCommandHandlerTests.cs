using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchCommands.VoidPunch;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Test.Common;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchCommands.VoidPunch;

[TestClass]
public class VoidPunchCommandHandlerTests : TestsBase
{
    private readonly string _rowVersion = "AAAAAAAAABA=";

    private Mock<IPunchRepository> _punchRepositoryMock;
    private Punch _existingPunch;

    private VoidPunchCommand _command;
    private VoidPunchCommandHandler _dut;

    [TestInitialize]
    public void Setup()
    {
        var project = new Project(TestPlantA, Guid.NewGuid(), "P", "D");
        _existingPunch = new Punch(TestPlantA, project, "Punch");
        _punchRepositoryMock = new Mock<IPunchRepository>();
        _punchRepositoryMock.Setup(r => r.TryGetByGuidAsync(_existingPunch.Guid))
            .ReturnsAsync(_existingPunch);

        _command = new VoidPunchCommand(_existingPunch.Guid, _rowVersion);

        _dut = new VoidPunchCommandHandler(
            _punchRepositoryMock.Object,
            _unitOfWorkMock.Object,
            new Mock<ILogger<VoidPunchCommandHandler>>().Object);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldVoidPunch()
    {
        // Arrange
        Assert.IsFalse(_existingPunch.IsVoided);

        // Act
        await _dut.Handle(_command, default);

        // Assert
        Assert.IsTrue(_existingPunch.IsVoided);
    }

    [TestMethod]
    public async Task HandlingCommand_ShouldSave()
    {
        // Act
        await _dut.Handle(_command, default);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
