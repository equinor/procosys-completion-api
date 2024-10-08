﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemAttachment;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.UpdatePunchItemAttachment;

[TestClass]
public class UpdatePunchItemAttachmentCommandValidatorTests : PunchItemCommandTestsBase
{
    private UpdatePunchItemAttachmentCommandValidator _dut;
    private IPunchItemValidator _punchItemValidatorMock;
    private IAttachmentService _attachmentServiceMock;
    private ILabelValidator _labelValidatorMock;
    private UpdatePunchItemAttachmentCommand _command;
    private readonly string _labelTextA = "a";
    private readonly string _labelTextB = "b";

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new UpdatePunchItemAttachmentCommand(
            _existingPunchItem[TestPlantA].Guid, 
            Guid.NewGuid(), 
            "d", 
            new List<string> { _labelTextA, _labelTextB }, 
            "r")
        {
            PunchItem = _existingPunchItem[TestPlantA],
            CheckListDetailsDto = new CheckListDetailsDto(
                _existingPunchItem[TestPlantA].CheckListGuid,
                "R",
                false,
                _existingPunchItem[TestPlantA].Project.Guid)
        };

        _punchItemValidatorMock = Substitute.For<IPunchItemValidator>();
        _attachmentServiceMock = Substitute.For<IAttachmentService>();
        _attachmentServiceMock.ExistsAsync(_command.AttachmentGuid, default)
            .Returns(true);
        _labelValidatorMock = Substitute.For<ILabelValidator>();
        _labelValidatorMock.ExistsAsync(_labelTextA, default).Returns(true);
        _labelValidatorMock.ExistsAsync(_labelTextB, default).Returns(true);
        _dut = new UpdatePunchItemAttachmentCommandValidator(
            _punchItemValidatorMock,
            _labelValidatorMock,
            _attachmentServiceMock);
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState()
    {
        var result = await _dut.ValidateAsync(_command);

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_AttachmentNotExists()
    {
        // Arrange
        _attachmentServiceMock.ExistsAsync(_command.AttachmentGuid, default)
            .Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Attachment with this guid does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_TagOwningPunchItemIsVoided()
    {
        // Arrange
        _command.CheckListDetailsDto = new CheckListDetailsDto(
            _existingPunchItem[TestPlantA].CheckListGuid,
            "R",
            true,
            _existingPunchItem[TestPlantA].Project.Guid);

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
    public async Task Validate_ShouldFail_When_PunchItemIsVerified_AndCurrentUserIsNotVerifier()
    {
        // Arrange
        _command.PunchItem.Clear(_currentPerson);
        _command.PunchItem.Verify(_existingPerson1);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Punch item attachments can't be changed. The punch item is verified!"));
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_When_PunchItemIsVerified_AndCurrentUserIsVerifier()
    {
        // Arrange
        _command.PunchItem.Clear(_currentPerson);
        _command.PunchItem.Verify(_existingPerson1);
        _punchItemValidatorMock.CurrentUserIsVerifier(_command.PunchItem)
            .Returns(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsTrue(result.IsValid);
    }
}
