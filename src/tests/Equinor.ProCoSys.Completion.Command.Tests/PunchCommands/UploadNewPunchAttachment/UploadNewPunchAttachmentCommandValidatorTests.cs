using System;
using System.IO;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchCommands.UploadNewPunchAttachment;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Command.Validators.PunchValidators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchCommands.UploadNewPunchAttachment;

[TestClass]
public class UploadNewPunchAttachmentCommandValidatorTests
{
    private UploadNewPunchAttachmentCommandValidator _dut;
    private Mock<IPunchValidator> _punchValidatorMock;
    private Mock<IAttachmentService> _attachmentServiceMock;
    private UploadNewPunchAttachmentCommand _command;

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new UploadNewPunchAttachmentCommand(Guid.NewGuid(), "f.txt", new MemoryStream());
        _punchValidatorMock = new Mock<IPunchValidator>();
        _punchValidatorMock.Setup(x => x.ExistsAsync(_command.PunchGuid, default))
            .ReturnsAsync(true);
        _attachmentServiceMock = new Mock<IAttachmentService>();
        _dut = new UploadNewPunchAttachmentCommandValidator(
            _punchValidatorMock.Object,
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
        _punchValidatorMock.Setup(inv => inv.ExistsAsync(_command.PunchGuid, default))
            .ReturnsAsync(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Punch with this guid does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_TagOwningPunchIsVoided()
    {
        // Arrange
        _punchValidatorMock.Setup(inv => inv.TagOwningPunchIsVoidedAsync(_command.PunchGuid, default))
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
        _punchValidatorMock.Setup(x => x.ProjectOwningPunchIsClosedAsync(_command.PunchGuid, default))
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
                _command.PunchGuid, 
                _command.FileName))
            .ReturnsAsync(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"Punch already has an attachment with filename {_command.FileName}! Please rename file or choose to overwrite"));
    }
}
