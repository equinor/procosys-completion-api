using System;
using System.IO;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchCommands.OverwriteExistingPunchAttachment;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Command.Validators.PunchValidators;
using Equinor.ProCoSys.Completion.Command.Validators.ProjectValidators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchCommands.OverwriteExistingPunchAttachment;

[TestClass]
public class OverwriteExistingPunchAttachmentCommandValidatorTests
{
    private OverwriteExistingPunchAttachmentCommandValidator _dut;
    private Mock<IPunchValidator> _punchValidatorMock;
    private Mock<IProjectValidator> _projectValidatorMock;
    private Mock<IAttachmentService> _attachmentServiceMock;
    private OverwriteExistingPunchAttachmentCommand _command;
    private readonly string _fileName = "a.txt";

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new OverwriteExistingPunchAttachmentCommand(Guid.NewGuid(), _fileName, "r", new MemoryStream());
        _projectValidatorMock = new Mock<IProjectValidator>();
        _punchValidatorMock = new Mock<IPunchValidator>();
        _punchValidatorMock.Setup(x => x.PunchExistsAsync(_command.PunchGuid, default))
            .ReturnsAsync(true);
        _attachmentServiceMock = new Mock<IAttachmentService>();
        _attachmentServiceMock.Setup(x => x.FilenameExistsForSourceAsync(
                _command.PunchGuid, 
                _command.FileName))
            .ReturnsAsync(true);
        _dut = new OverwriteExistingPunchAttachmentCommandValidator(
            _projectValidatorMock.Object,
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
        _punchValidatorMock.Setup(inv => inv.PunchExistsAsync(_command.PunchGuid, default))
            .ReturnsAsync(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Punch with this guid does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_PunchIsVoided()
    {
        // Arrange
        _punchValidatorMock.Setup(inv => inv.PunchIsVoidedAsync(_command.PunchGuid, default))
            .ReturnsAsync(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Punch is voided!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_ProjectIsClosed()
    {
        // Arrange
        _projectValidatorMock.Setup(x => x.IsClosedForPunch(_command.PunchGuid, default))
            .ReturnsAsync(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Project is closed!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_AttachmentWithFilenameNotExists()
    {
        // Arrange
        _attachmentServiceMock.Setup(x => x.FilenameExistsForSourceAsync(
                _command.PunchGuid,
                _command.FileName))
            .ReturnsAsync(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"Punch don't have an attachment with filename {_command.FileName}!"));
    }
}
