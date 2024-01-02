using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.WebApi.Controllers.Comments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Controllers.Comments;

[TestClass]
public class CreateCommentDtoValidatorTests
{
    private readonly CreateCommentDtoValidator _dut = new();

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState()
    {
        // Arrange
        var dto = new CreateCommentDto("New text", new List<string>());

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenTextNotGiven()
    {
        // Arrange
        var dto = new CreateCommentDto(null!, new List<string>());

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("'Text' must not be empty."));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenTextIsEmpty()
    {
        // Arrange
        var dto = new CreateCommentDto(string.Empty, new List<string>());

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("The length of 'Text' must be at least"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenTextIsTooLongAsync()
    {
        // Arrange
        var dto = new CreateCommentDto(
            new string('x', Domain.AggregateModels.CommentAggregate.Comment.TextLengthMax + 1), 
            new List<string>());

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("The length of 'Text' must be"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenListOfLabelsNotGiven()
    {
        // Arrange
        var dto = new CreateCommentDto("New text", null!);

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
        var dto = new CreateCommentDto(
            "New test",
            new List<string> { "a", null! });

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
        var dto = new CreateCommentDto(
            "New test",
            new List<string> { "a", "a" });

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Labels must be unique!"));
    }
}
