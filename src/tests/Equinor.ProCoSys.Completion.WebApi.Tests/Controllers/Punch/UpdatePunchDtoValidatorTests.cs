using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.WebApi.Controllers;
using Equinor.ProCoSys.Completion.WebApi.Controllers.Punch;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Controllers.Punch;

[TestClass]
public class UpdatePunchDtoValidatorTests
{
    private readonly string _rowVersion = "AAAAAAAAABA=";

    private UpdatePunchDtoValidator _dut;
    private Mock<IRowVersionValidator> _rowVersionValidatorMock;

    [TestInitialize]
    public void Setup_OkState()
    {
        _rowVersionValidatorMock = new Mock<IRowVersionValidator>();
        _rowVersionValidatorMock.Setup(x => x.IsValid(_rowVersion)).Returns(true);

        _dut = new UpdatePunchDtoValidator(_rowVersionValidatorMock.Object);
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState()
    {
        // Arrange
        var dto = new UpdatePunchDto(null, _rowVersion);
        
        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenRowVersionNotGiven()
    {
        // Arrange
        var dto = new UpdatePunchDto(null, null!);

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
        _rowVersionValidatorMock.Setup(x => x.IsValid(_rowVersion)).Returns(false);
        var dto = new UpdatePunchDto(null, _rowVersion);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Dto does not have valid rowVersion!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenDescriptionIsTooLongAsync()
    {
        // Arrange
        var dto = new UpdatePunchDto(
            new string('x', Domain.AggregateModels.PunchAggregate.Punch.DescriptionLengthMax + 1),
            _rowVersion);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("The length of 'Description' must be"));
    }
}
