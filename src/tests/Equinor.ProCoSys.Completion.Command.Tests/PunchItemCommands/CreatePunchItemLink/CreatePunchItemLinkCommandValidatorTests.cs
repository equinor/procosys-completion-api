using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemLink;
using Equinor.ProCoSys.Completion.Command.Validators.PunchItemValidators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.CreatePunchItemLink;

[TestClass]
public class CreatePunchItemLinkCommandValidatorTests
{
    private CreatePunchItemLinkCommandValidator _dut;
    private Mock<IPunchItemValidator> _punchItemValidatorMock;
    private CreatePunchItemLinkCommand _command;

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new CreatePunchItemLinkCommand(Guid.NewGuid(), "Test title", "www");
        _punchItemValidatorMock = new Mock<IPunchItemValidator>();
        _punchItemValidatorMock.Setup(x => x.ExistsAsync(_command.PunchItemGuid, default))
            .ReturnsAsync(true);
        _dut = new CreatePunchItemLinkCommandValidator(_punchItemValidatorMock.Object);
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
        _punchItemValidatorMock.Setup(inv => inv.ExistsAsync(_command.PunchItemGuid, default))
            .ReturnsAsync(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Punch item with this guid does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_TagOwningPunchIsVoided()
    {
        // Arrange
        _punchItemValidatorMock.Setup(inv => inv.TagOwningPunchIsVoidedAsync(_command.PunchItemGuid, default))
            .ReturnsAsync(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Tag owning punch item is voided!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_ProjectIsClosed()
    {
        // Arrange
        _punchItemValidatorMock.Setup(x => x.ProjectOwningPunchIsClosedAsync(_command.PunchItemGuid, default))
            .ReturnsAsync(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Project is closed!"));
    }
}
