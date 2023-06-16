using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchCommands.CreatePunch;
using Equinor.ProCoSys.Completion.Command.Validators.ProjectValidators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchCommands.CreatePunch;

[TestClass]
public class CreatePunchCommandValidatorTests
{
    private CreatePunchCommandValidator _dut;
    private CreatePunchCommand _command;
    private Mock<IProjectValidator> _projectValidatorMock;

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new CreatePunchCommand("Test title", "Project name");
        _projectValidatorMock = new Mock<IProjectValidator>();
        _projectValidatorMock.Setup(x => x.ExistsAsync(_command.ProjectName, default))
            .ReturnsAsync(true);
        _dut = new CreatePunchCommandValidator(_projectValidatorMock.Object);
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState()
    {
        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_ProjectNotExists()
    {
        // Arrange
        _projectValidatorMock.Setup(x => x.ExistsAsync(_command.ProjectName, default))
            .ReturnsAsync(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Project with this name does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_ProjectIsClosed()
    {
        // Arrange
        _projectValidatorMock.Setup(x => x.IsClosed(_command.ProjectName, default))
            .ReturnsAsync(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Project is closed!"));
    }
}
