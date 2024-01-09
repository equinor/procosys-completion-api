using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemComment;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.CreatePunchItemComment;

[TestClass]
public class CreatePunchItemCommentCommandValidatorTests
{
    private CreatePunchItemCommentCommandValidator _dut;
    private IPunchItemValidator _punchItemValidatorMock;
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
            new List<Guid>());
        _punchItemValidatorMock = Substitute.For<IPunchItemValidator>();
        _punchItemValidatorMock.ExistsAsync(_command.PunchItemGuid, default).Returns(true);
        _labelValidatorMock = Substitute.For<ILabelValidator>();
        _labelValidatorMock.ExistsAsync(_labelTextA, default).Returns(true);
        _labelValidatorMock.ExistsAsync(_labelTextB, default).Returns(true);
        _dut = new CreatePunchItemCommentCommandValidator(_punchItemValidatorMock, _labelValidatorMock);
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState()
    {
        var result = await _dut.ValidateAsync(_command);

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_PunchItemNotExists()
    {
        // Arrange
        _punchItemValidatorMock.ExistsAsync(_command.PunchItemGuid, default)
            .Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Punch item with this guid does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_TagOwningPunchItemIsVoided()
    {
        // Arrange
        _punchItemValidatorMock.TagOwningPunchItemIsVoidedAsync(_command.PunchItemGuid, default)
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
        _punchItemValidatorMock.ProjectOwningPunchItemIsClosedAsync(_command.PunchItemGuid, default)
            .Returns(true);

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
        _punchItemValidatorMock.IsVerifiedAsync(_command.PunchItemGuid, default)
            .Returns(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Punch item is verified!"));
    }
}
