using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.WebApi.Controllers.Punch;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Controllers.Punch;

[TestClass]
public class CreatePunchDtoValidatorTests
{
    private readonly CreatePunchDtoValidator _dut = new();

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState()
    {
        // Arrange
        var dto = new CreatePunchDto { ItemNo = "New item"};
        
        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenItemNoNotGiven()
    {
        // Arrange
        var dto = new CreatePunchDto();

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
        var dto = new CreatePunchDto { ItemNo = "N" };

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
        var dto = new CreatePunchDto
        {
            ItemNo = new string('x', Domain.AggregateModels.PunchAggregate.Punch.ItemNoLengthMax + 1)
        };

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("The length of 'Item No' must be"));
    }
}
