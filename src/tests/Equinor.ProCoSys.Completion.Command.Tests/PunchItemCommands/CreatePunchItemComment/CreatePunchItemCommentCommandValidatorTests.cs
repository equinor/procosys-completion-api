using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemComment;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.CreatePunchItemComment;

[TestClass]
public class CreatePunchItemCommentCommandValidatorTests : PunchItemCommandTestsBase
{
    private CreatePunchItemCommentCommandValidator _dut;
    private ILabelValidator _labelValidatorMock;
    private CreatePunchItemCommentCommand _command;
    private readonly string _labelTextA = "a";
    private readonly string _labelTextB = "b";

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new CreatePunchItemCommentCommand(
            Guid.NewGuid(),
            "Test title",
            new List<string> { _labelTextA, _labelTextB },
            new List<Guid>())
        {
            PunchItem = _existingPunchItem[TestPlantA]
        };

        _labelValidatorMock = Substitute.For<ILabelValidator>();
        _labelValidatorMock.ExistsAsync(_labelTextA, default).Returns(true);
        _labelValidatorMock.ExistsAsync(_labelTextB, default).Returns(true);
        _dut = new CreatePunchItemCommentCommandValidator(_checkListValidatorMock, _labelValidatorMock);
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState()
    {
        var result = await _dut.ValidateAsync(_command);

        Assert.IsTrue(result.IsValid);
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
    public async Task Validate_ShouldFail_When_ALabelNotExists()
    {
        // Arrange
        _labelValidatorMock.ExistsAsync(_labelTextA, default).Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"Label doesn't exist! Label={_labelTextA}"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_ALabelIsVoided()
    {
        // Arrange
        _labelValidatorMock.IsVoidedAsync(_labelTextA, default).Returns(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"Label is voided! Label={_labelTextA}"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_PunchItemIsVerified()
    {
        // Arrange
        _command.PunchItem.Clear(_currentPerson);
        _command.PunchItem.Verify(_currentPerson);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Punch item comments can't be added. Punch item is verified!"));
    }
}
