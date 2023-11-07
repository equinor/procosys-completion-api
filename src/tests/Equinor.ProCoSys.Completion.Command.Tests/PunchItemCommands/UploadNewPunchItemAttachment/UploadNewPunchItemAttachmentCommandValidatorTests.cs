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
public class UploadNewPunchItemAttachmentCommandValidatorTests
{
    private UploadNewPunchItemAttachmentCommandValidator _dut;
    private IPunchItemValidator _punchItemValidatorMock;
    private IAttachmentService _attachmentServiceMock;
    private UploadNewPunchItemAttachmentCommand _command;

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new UploadNewPunchItemAttachmentCommand(Guid.NewGuid(), "f.txt", new MemoryStream());
        _punchItemValidatorMock = Substitute.For<IPunchItemValidator>();
        _punchItemValidatorMock.ExistsAsync(_command.PunchItemGuid, default)
            .Returns(true);
        _attachmentServiceMock = Substitute.For<IAttachmentService>();
        _dut = new UploadNewPunchItemAttachmentCommandValidator(
            _punchItemValidatorMock,
            _attachmentServiceMock);
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
}
