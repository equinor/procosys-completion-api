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
        var dto = new CreatePunchDto { Title = "New title", ProjectName = "P" };
        
        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenTitleNotGiven()
    {
        // Arrange
        var dto = new CreatePunchDto { ProjectName = "P" };

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("'Title' must not be empty."));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenTitleIsTooShort()
    {
        // Arrange
        var dto = new CreatePunchDto { Title = "N", ProjectName = "P" };

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("The length of 'Title' must be"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenTitleIsTooLongAsync()
    {
        // Arrange
        var dto = new CreatePunchDto
        {
            Title = new string('x', Domain.AggregateModels.PunchAggregate.Punch.TitleLengthMax + 1),
            ProjectName = "P"
        };

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("The length of 'Title' must be"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenProjectNotGiven()
    {
        // Arrange
        var dto = new CreatePunchDto { Title = "New title" };

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("'Project Name' must not be empty."));
    }
}
