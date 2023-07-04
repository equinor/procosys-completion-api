using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Controllers.PunchItem;

[TestClass]
public class CreatePunchItemDtoValidatorTests
{
    private readonly CreatePunchItemDtoValidator _dut = new();

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState()
    {
        // Arrange
        var dto = new CreatePunchItemDto("New item", Guid.Empty);
        
        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenItemNoNotGiven()
    {
        // Arrange
        var dto = new CreatePunchItemDto(null!, Guid.NewGuid());

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("'Item No' must not be empty."));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenItemNoIsTooShort()
    {
        // Arrange
        var dto = new CreatePunchItemDto("N", Guid.Empty);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"The length of 'Item No' must be"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenItemNoIsTooLongAsync()
    {
        // Arrange
        var dto = new CreatePunchItemDto(
            new string('x', Domain.AggregateModels.PunchItemAggregate.PunchItem.ItemNoLengthMax + 1),
            Guid.NewGuid());

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("The length of 'Item No' must be"));
    }
}
