using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Attachments;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItemAttachment;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.DeletePunchItemAttachment;

[TestClass]
public class DeletePunchItemAttachmentCommandValidatorTests : PunchItemCommandTestsBase
{
    private DeletePunchItemAttachmentCommandValidator _dut;
    private IPunchItemValidator _punchItemValidatorMock;
    private IAttachmentService _attachmentServiceMock;
    private DeletePunchItemAttachmentCommand _command;

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new DeletePunchItemAttachmentCommand(_existingPunchItem[TestPlantA].Guid, Guid.NewGuid(), "r")
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
        _dut = new DeletePunchItemAttachmentCommandValidator(
            _punchItemValidatorMock,
            _attachmentServiceMock);
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
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Punch item attachments can't be deleted. The punch item is verified!"));
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
