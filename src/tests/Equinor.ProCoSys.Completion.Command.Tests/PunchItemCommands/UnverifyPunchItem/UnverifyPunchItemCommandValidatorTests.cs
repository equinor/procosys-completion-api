﻿using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnverifyPunchItem;
using Equinor.ProCoSys.Completion.Command.Validators.PunchItemValidators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.UnverifyPunchItem;

[TestClass]
public class UnverifyPunchItemCommandValidatorTests
{
    private UnverifyPunchItemCommandValidator _dut;
    private Mock<IPunchItemValidator> _punchItemValidatorMock;
    private UnverifyPunchItemCommand _command;

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new UnverifyPunchItemCommand(Guid.NewGuid(), "r");
        _punchItemValidatorMock = new Mock<IPunchItemValidator>();
        _punchItemValidatorMock.Setup(x => x.ExistsAsync(_command.PunchItemGuid, default))
            .ReturnsAsync(true);
        _punchItemValidatorMock.Setup(x => x.IsVerifiedAsync(_command.PunchItemGuid, default))
            .ReturnsAsync(true);

        _dut = new UnverifyPunchItemCommandValidator(_punchItemValidatorMock.Object);
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
    public async Task Validate_ShouldFail_When_PunchItemNotExists()
    {
        // Arrange
        _punchItemValidatorMock.Setup(x => x.ExistsAsync(_command.PunchItemGuid, default))
            .ReturnsAsync(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Punch item with this guid does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_PunchItemIsVoided()
    {
        // Arrange
        _punchItemValidatorMock.Setup(x => x.TagOwningPunchItemIsVoidedAsync(_command.PunchItemGuid, default))
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
        _punchItemValidatorMock.Setup(x => x.ProjectOwningPunchItemIsClosedAsync(_command.PunchItemGuid, default))
            .ReturnsAsync(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Project is closed!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_PunchItemNotVerified()
    {
        // Arrange
        _punchItemValidatorMock.Setup(x => x.IsVerifiedAsync(_command.PunchItemGuid, default))
            .ReturnsAsync(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Punch item can not be unverified. The punch item is not verified!"));
    }
}