using System;
using System.IO;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UploadNewPunchItemAttachment;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Command.Validators.PunchItemValidators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.UploadNewPunchItemAttachment;

[TestClass]
public class UploadNewPunchItemAttachmentCommandValidatorTests
{
    private UploadNewPunchItemAttachmentCommandValidator _dut;
    private Mock<IPunchItemValidator> _punchItemValidatorMock;
    private Mock<IAttachmentService> _attachmentServiceMock;
    private UploadNewPunchItemAttachmentCommand _command;

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new UploadNewPunchItemAttachmentCommand(Guid.NewGuid(), "f.txt", new MemoryStream());
        _punchItemValidatorMock = new Mock<IPunchItemValidator>();
        _punchItemValidatorMock.Setup(x => x.ExistsAsync(_command.PunchItemGuid, default))
            .ReturnsAsync(true);
        _attachmentServiceMock = new Mock<IAttachmentService>();
        _dut = new UploadNewPunchItemAttachmentCommandValidator(
            _punchItemValidatorMock.Object,
            _attachmentServiceMock.Object);
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
    public async Task Validate_ShouldFail_When_TagOwningPunchItemIsVoided()
    {
        // Arrange
        _punchItemValidatorMock.Setup(inv => inv.TagOwningPunchItemIsVoidedAsync(_command.PunchItemGuid, default))
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
    public async Task Validate_ShouldFail_When_AttachmentWithFilenameExists()
    {
        // Arrange
        _attachmentServiceMock.Setup(x => x.FileNameExistsForSourceAsync(
                _command.PunchItemGuid, 
                _command.FileName))
            .ReturnsAsync(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"Punch item already has an attachment with filename {_command.FileName}! Please rename file or choose to overwrite"));
    }
}
