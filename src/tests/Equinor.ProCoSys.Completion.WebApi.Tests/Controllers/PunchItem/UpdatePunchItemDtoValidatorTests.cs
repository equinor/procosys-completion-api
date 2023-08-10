using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.WebApi.Controllers;
using Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Controllers.PunchItem;

[TestClass]
public class UpdatePunchItemDtoValidatorTests
{
    private readonly string _rowVersion = "AAAAAAAAABA=";

    private UpdatePunchItemDtoValidator _dut;
    private IRowVersionValidator _rowVersionValidatorMock;

    [TestInitialize]
    public void Setup_OkState()
    {
        _rowVersionValidatorMock = Substitute.For<IRowVersionValidator>();
        _rowVersionValidatorMock.IsValid(_rowVersion).Returns(true);

        _dut = new UpdatePunchItemDtoValidator(_rowVersionValidatorMock);
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState()
    {
        // Arrange
        var dto = new UpdatePunchItemDto(null, _rowVersion);
        
        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenRowVersionNotGiven()
    {
        // Arrange
        var dto = new UpdatePunchItemDto(null, null!);

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
        var dto = new UpdatePunchItemDto(null, _rowVersion);

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
        var dto = new UpdatePunchItemDto(
            new string('x', Domain.AggregateModels.PunchItemAggregate.PunchItem.DescriptionLengthMax + 1),
            _rowVersion);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("The length of 'Description' must be"));
    }
}
