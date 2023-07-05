using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.WebApi.Controllers.Links;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Controllers.Links;

[TestClass]
public class CreateLinkDtoValidatorTests
{
    private readonly CreateLinkDtoValidator _dut = new();

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState()
    {
        // Arrange
        var dto = new CreateLinkDto("New title", "U");

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenTitleNotGiven()
    {
        // Arrange
        var dto = new CreateLinkDto(null!, "U");

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("'Title' must not be empty."));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenTitleIsTooLongAsync()
    {
        // Arrange
        var dto = new CreateLinkDto(
            new string('x', Domain.AggregateModels.LinkAggregate.Link.TitleLengthMax + 1),
            "U");

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("The length of 'Title' must be"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenUrlNotGiven()
    {
        // Arrange
        var dto = new CreateLinkDto("New title", null!);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("'Url' must not be empty."));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenUrlIsTooLongAsync()
    {
        // Arrange
        var dto = new CreateLinkDto(
            "New title",
            new string('x', Domain.AggregateModels.LinkAggregate.Link.UrlLengthMax + 1));

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("The length of 'Url' must be"));
    }
}
