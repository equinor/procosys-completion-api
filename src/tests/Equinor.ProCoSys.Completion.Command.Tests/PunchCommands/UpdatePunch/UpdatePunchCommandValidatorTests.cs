using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchCommands.UpdatePunch;
using Equinor.ProCoSys.Completion.Command.Validators.PunchValidators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchCommands.UpdatePunch;

[TestClass]
public class UpdatePunchCommandValidatorTests
{
    private UpdatePunchCommandValidator _dut;
    private Mock<IPunchValidator> _punchValidatorMock;
    private UpdatePunchCommand _command;

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new UpdatePunchCommand(Guid.NewGuid(), "New description", "r");
        _punchValidatorMock = new Mock<IPunchValidator>();
        _punchValidatorMock.Setup(x => x.ExistsAsync(_command.PunchGuid, default))
            .ReturnsAsync(true);

        _dut = new UpdatePunchCommandValidator(_punchValidatorMock.Object);
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
    public async Task Validate_ShouldFail_When_PunchNotExists()
    {
        // Arrange
        _punchValidatorMock.Setup(inv => inv.ExistsAsync(_command.PunchGuid, default))
            .ReturnsAsync(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Punch with this guid does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_TagOwningPunchIsVoided()
    {
        // Arrange
        _punchValidatorMock.Setup(inv => inv.TagOwningPunchIsVoidedAsync(_command.PunchGuid, default))
            .ReturnsAsync(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Tag owning punch is voided!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_ProjectIsClosed()
    {
        // Arrange
        _punchValidatorMock.Setup(x => x.ProjectOwningPunchIsClosedAsync(_command.PunchGuid, default))
            .ReturnsAsync(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Project is closed!"));
    }
}
