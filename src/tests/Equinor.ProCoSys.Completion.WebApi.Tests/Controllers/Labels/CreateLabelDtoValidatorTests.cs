using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.WebApi.Controllers.Labels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Controllers.Labels;

[TestClass]
public class CreateLabelDtoValidatorTests
{
    private readonly CreateLabelDtoValidator _dut = new();

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState()
    {
        // Arrange
        var dto = new CreateLabelDto("New");

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenTextNotGiven()
    {
        // Arrange
        var dto = new CreateLabelDto(null!);

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
        var dto = new CreateLabelDto(string.Empty);

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
        var dto = new CreateLabelDto(new string('x', Domain.AggregateModels.LabelAggregate.Label.TextLengthMax + 1));

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("The length of 'Text' must be"));
    }
}
