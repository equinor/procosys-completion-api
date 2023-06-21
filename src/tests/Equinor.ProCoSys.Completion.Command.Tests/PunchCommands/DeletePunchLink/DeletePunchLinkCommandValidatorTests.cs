using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Links;
using Equinor.ProCoSys.Completion.Command.PunchCommands.DeletePunchLink;
using Equinor.ProCoSys.Completion.Command.Validators.PunchValidators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Equinor.ProCoSys.Completion.Command.Validators.ProjectValidators;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchCommands.DeletePunchLink;

[TestClass]
public class DeletePunchLinkCommandValidatorTests
{
    private DeletePunchLinkCommandValidator _dut;
    private Mock<IPunchValidator> _punchValidatorMock;
    private Mock<IProjectValidator> _projectValidatorMock;
    private Mock<ILinkService> _linkServiceMock;

    private DeletePunchLinkCommand _command;

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new DeletePunchLinkCommand(Guid.NewGuid(), Guid.NewGuid(), "r1");
        _projectValidatorMock = new Mock<IProjectValidator>();
        _punchValidatorMock = new Mock<IPunchValidator>();
        _punchValidatorMock.Setup(x => x.PunchExistsAsync(_command.PunchGuid, default))
            .ReturnsAsync(true);
        _linkServiceMock = new Mock<ILinkService>();
        _linkServiceMock.Setup(x => x.ExistsAsync(_command.LinkGuid))
            .ReturnsAsync(true);

        _dut = new DeletePunchLinkCommandValidator(
            _projectValidatorMock.Object,
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
