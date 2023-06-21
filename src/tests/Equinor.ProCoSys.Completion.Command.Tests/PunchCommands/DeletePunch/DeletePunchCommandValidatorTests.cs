using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchCommands.DeletePunch;
using Equinor.ProCoSys.Completion.Command.Validators.PunchValidators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchCommands.DeletePunch;

[TestClass]
public class DeletePunchCommandValidatorTests
{
    private DeletePunchCommandValidator _dut;
    private Mock<IPunchValidator> _punchValidatorMock;
    private DeletePunchCommand _command;

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new DeletePunchCommand(Guid.NewGuid(), "r");
        _punchValidatorMock = new Mock<IPunchValidator>();
        _punchValidatorMock.Setup(x => x.PunchExistsAsync(_command.PunchGuid, default)).ReturnsAsync(true);
        _punchValidatorMock.Setup(x => x.TagOwingPunchIsVoidedAsync(_command.PunchGuid, default)).ReturnsAsync(true);

        _dut = new DeletePunchCommandValidator(_punchValidatorMock.Object);
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
