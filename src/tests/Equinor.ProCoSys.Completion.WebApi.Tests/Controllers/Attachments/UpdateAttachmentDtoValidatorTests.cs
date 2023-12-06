using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.WebApi.Controllers.Attachments;
using Equinor.ProCoSys.Completion.WebApi.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Controllers.Attachments;

[TestClass]
public class UpdateAttachmentDtoValidatorTests
{
    private readonly string _rowVersion = "AAAAAAAAABA=";

    private UpdateAttachmentDtoValidator _dut = null!;
    private IRowVersionInputValidator _rowVersionValidatorMock = null!;

    [TestInitialize]
    public void Setup_OkState()
    {
        _rowVersionValidatorMock = Substitute.For<IRowVersionInputValidator>();
        _rowVersionValidatorMock.IsValid(_rowVersion).Returns(true);

        _dut = new UpdateAttachmentDtoValidator(_rowVersionValidatorMock);
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState()
    {
        // Arrange
        var dto = new UpdateAttachmentDto("New desc", new List<string>(), _rowVersion);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenDescriptionNotGiven()
    {
        // Arrange
        var dto = new UpdateAttachmentDto(null!, new List<string>(), _rowVersion);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("'Description' must not be empty."));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenDescriptionIsTooLong()
    {
        // Arrange
        var dto = new UpdateAttachmentDto(
            new string('x', Domain.AggregateModels.AttachmentAggregate.Attachment.DescriptionLengthMax + 1),
            new List<string>(),
            _rowVersion);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("The length of 'Description' must be"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenListOfLabelsNotGiven()
    {
        // Arrange
        var dto = new UpdateAttachmentDto("New description", null!, _rowVersion);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("'Labels' must not be empty."));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenALabelIsNull()
    {
        // Arrange
        var dto = new UpdateAttachmentDto(
            "New description",
            new List<string>{"a", null!},
            _rowVersion);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("'Labels' must not be empty."));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenALabelNotUnique()
    {
        // Arrange
        var dto = new UpdateAttachmentDto(
            "New description",
            new List<string> { "a", "a" },
            _rowVersion);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Labels must be unique!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenRowVersionNotGiven()
    {
        // Arrange
        var dto = new UpdateAttachmentDto("New description", new List<string>(), null!);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("'Row Version' must not be empty."));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenIllegalRowVersion()
    {
        // Arrange
        _rowVersionValidatorMock.IsValid(_rowVersion).Returns(false);
        var dto = new UpdateAttachmentDto("New description", new List<string>(), _rowVersion);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Dto does not have valid rowVersion!"));
    }
}
