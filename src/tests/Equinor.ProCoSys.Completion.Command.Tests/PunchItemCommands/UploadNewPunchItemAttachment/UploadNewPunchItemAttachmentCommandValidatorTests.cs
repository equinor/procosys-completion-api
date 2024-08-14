using System;
using System.IO;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UploadNewPunchItemAttachment;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.UploadNewPunchItemAttachment;

[TestClass]
public class UploadNewPunchItemAttachmentCommandValidatorTests : PunchItemCommandTestsBase
{
    private UploadNewPunchItemAttachmentCommandValidator _dut;
    private IPunchItemValidator _punchItemValidatorMock;
    private IAttachmentService _attachmentServiceMock;
    private UploadNewPunchItemAttachmentCommand _command;

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new UploadNewPunchItemAttachmentCommand(Guid.NewGuid(), "f.txt", new MemoryStream(), "image/jpeg")
        {
            PunchItem = _existingPunchItem[TestPlantA]
        };

        _punchItemValidatorMock = Substitute.For<IPunchItemValidator>();
        _attachmentServiceMock = Substitute.For<IAttachmentService>();
        _dut = new UploadNewPunchItemAttachmentCommandValidator(
            _punchItemValidatorMock,
            _checkListValidatorMock,
            _attachmentServiceMock);
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
    public async Task Validate_ShouldFail_When_AttachmentWithFilenameExists()
    {
        // Arrange
        _attachmentServiceMock.FileNameExistsForParentAsync(
                _command.PunchItemGuid, 
                _command.FileName, 
                default)
            .Returns(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"Punch item already has an attachment with filename {_command.FileName}! Please rename file or choose to overwrite"));
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
