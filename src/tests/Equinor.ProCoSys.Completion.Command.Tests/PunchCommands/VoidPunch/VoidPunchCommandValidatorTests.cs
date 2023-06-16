using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchCommands.VoidPunch;
using Equinor.ProCoSys.Completion.Command.Validators.PunchValidators;
using Equinor.ProCoSys.Completion.Command.Validators.ProjectValidators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchCommands.VoidPunch;

[TestClass]
public class VoidPunchCommandValidatorTests
{
    private VoidPunchCommandValidator _dut;
    private Mock<IPunchValidator> _punchValidatorMock;
    private Mock<IProjectValidator> _projectValidatorMock;
    private VoidPunchCommand _command;

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new VoidPunchCommand(Guid.NewGuid(), "r");
        _projectValidatorMock = new Mock<IProjectValidator>();
        _punchValidatorMock = new Mock<IPunchValidator>();
        _punchValidatorMock.Setup(x => x.PunchExistsAsync(_command.PunchGuid, default)).ReturnsAsync(true);

        _dut = new VoidPunchCommandValidator(_projectValidatorMock.Object, _punchValidatorMock.Object);
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
    public async Task Validate_ShouldFail_When_PunchAlreadyVoided()
    {
        // Arrange
        _punchValidatorMock.Setup(x => x.PunchIsVoidedAsync(_command.PunchGuid, default)).ReturnsAsync(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Punch is already voided!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_PunchNotExists()
    {
        // Arrange
        _punchValidatorMock.Setup(inv => inv.PunchExistsAsync(_command.PunchGuid, default))
            .ReturnsAsync(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Punch with this guid does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_ProjectIsClosed()
    {
        // Arrange
        _projectValidatorMock.Setup(x => x.IsClosedForPunch(_command.PunchGuid, default))
            .ReturnsAsync(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Project is closed!"));
    }
}
