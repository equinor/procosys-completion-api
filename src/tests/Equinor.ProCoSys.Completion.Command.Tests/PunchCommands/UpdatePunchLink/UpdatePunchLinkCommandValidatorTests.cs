using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Links;
using Equinor.ProCoSys.Completion.Command.PunchCommands.UpdatePunchLink;
using Equinor.ProCoSys.Completion.Command.Validators.PunchValidators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchCommands.UpdatePunchLink;

[TestClass]
public class UpdatePunchLinkCommandValidatorTests
{
    private UpdatePunchLinkCommandValidator _dut;
    private Mock<IPunchValidator> _punchValidatorMock;
    private Mock<ILinkService> _linkServiceMock;
    private UpdatePunchLinkCommand _command;

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new UpdatePunchLinkCommand(Guid.NewGuid(), Guid.NewGuid(), "New title", "New text", "r");
        _punchValidatorMock = new Mock<IPunchValidator>();
        _punchValidatorMock.Setup(x => x.ExistsAsync(_command.PunchGuid, default))
            .ReturnsAsync(true);
        _linkServiceMock = new Mock<ILinkService>();
        _linkServiceMock.Setup(x => x.ExistsAsync(_command.LinkGuid))
            .ReturnsAsync(true);

        _dut = new UpdatePunchLinkCommandValidator(
            _punchValidatorMock.Object,
            _linkServiceMock.Object);
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
    public async Task Validate_ShouldFail_When_LinkNotExists()
    {
        // Arrange
        _linkServiceMock.Setup(x => x.ExistsAsync(_command.LinkGuid))
            .ReturnsAsync(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Link with this guid does not exist!"));
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
