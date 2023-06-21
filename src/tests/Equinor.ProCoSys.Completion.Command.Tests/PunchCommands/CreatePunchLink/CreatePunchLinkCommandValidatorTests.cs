using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchCommands.CreatePunchLink;
using Equinor.ProCoSys.Completion.Command.Validators.PunchValidators;
using Equinor.ProCoSys.Completion.Command.Validators.ProjectValidators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchCommands.CreatePunchLink;

[TestClass]
public class CreatePunchLinkCommandValidatorTests
{
    private CreatePunchLinkCommandValidator _dut;
    private Mock<IPunchValidator> _punchValidatorMock;
    private Mock<IProjectValidator> _projectValidatorMock;
    private CreatePunchLinkCommand _command;

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new CreatePunchLinkCommand(Guid.NewGuid(), "Test title", "www");
        _projectValidatorMock = new Mock<IProjectValidator>();
        _punchValidatorMock = new Mock<IPunchValidator>();
        _punchValidatorMock.Setup(x => x.PunchExistsAsync(_command.PunchGuid, default))
            .ReturnsAsync(true);
        _dut = new CreatePunchLinkCommandValidator(_projectValidatorMock.Object, _punchValidatorMock.Object);
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState()
    {
        var result = await _dut.ValidateAsync(_command);

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
    public async Task Validate_ShouldFail_When_PunchIsVoided()
    {
        // Arrange
        _punchValidatorMock.Setup(inv => inv.TagOwingPunchIsVoidedAsync(_command.PunchGuid, default))
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
