using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Links;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemLink;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.UpdatePunchItemLink;

[TestClass]
public class UpdatePunchItemLinkCommandValidatorTests : PunchItemCommandTestsBase
{
    private UpdatePunchItemLinkCommandValidator _dut;
    private ILinkService _linkServiceMock;
    private UpdatePunchItemLinkCommand _command;

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new UpdatePunchItemLinkCommand(Guid.NewGuid(), Guid.NewGuid(), "New title", "New text", "r")
        {
            PunchItem = _existingPunchItem[TestPlantA]
        };

        _linkServiceMock = Substitute.For<ILinkService>();
        _linkServiceMock.ExistsAsync(_command.LinkGuid, default)
            .Returns(true);

        _dut = new UpdatePunchItemLinkCommandValidator(
            _checkListValidatorMock,
            _linkServiceMock);
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
    public async Task Validate_ShouldFail_When_LinkNotExists()
    {
        // Arrange
        _linkServiceMock.ExistsAsync(_command.LinkGuid, default)
            .Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Link with this guid does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_TagOwningPunchItemIsVoided()
    {
        // Arrange
        _checkListValidatorMock.TagOwningCheckListIsVoidedAsync(_command.PunchItem.CheckListGuid, default)
            .Returns(true);

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
        _command.PunchItem.Project.IsClosed = true;

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Project is closed!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_PunchItemIsCleared()
    {
        // Arrange
        _command.PunchItem.Clear(_currentPerson);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Punch item links can't be updated. Punch item is cleared!"));
    }
}
