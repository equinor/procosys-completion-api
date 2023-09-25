using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
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
        var dto = new CreatePunchItemDto(Category.PA, "New item", Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
        
        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenDescriptionNotGiven()
    {
        // Arrange
        var dto = new CreatePunchItemDto(Category.PA, null!, Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("'Description' must not be empty."));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenDescriptionIsTooLongAsync()
    {
        // Arrange
        var dto = new CreatePunchItemDto(Category.PA,
            new string('x', Domain.AggregateModels.PunchItemAggregate.PunchItem.DescriptionLengthMax + 1),
            Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("The length of 'Description' must be"));
    }
}
