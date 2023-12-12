using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Equinor.ProCoSys.Completion.WebApi.Controllers.Labels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Controllers.Labels;

[TestClass]
public class UpdateLabelAvailableForDtoValidatorTests
{
    private readonly UpdateLabelAvailableForDtoValidator _dut = new();

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState()
    {
        // Arrange
        var dto = new UpdateLabelAvailableForDto("New", new List<EntityTypeWithLabel>());

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenTextNotGiven()
    {
        // Arrange
        var dto = new UpdateLabelAvailableForDto(null!, new List<EntityTypeWithLabel>());

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("'Text' must not be empty."));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenAvailableForLabelsNotGiven()
    {
        // Arrange
        var dto = new UpdateLabelAvailableForDto("New", null!);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("'Available For Labels' must not be empty."));
    }
}
