using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.WebApi.Controllers.Links;
using Equinor.ProCoSys.Completion.WebApi.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Controllers.Links;

[TestClass]
public class UpdateLinkDtoValidatorTests
{
    private readonly string _rowVersion = "AAAAAAAAABA=";

    private UpdateLinkDtoValidator _dut = null!;
    private IRowVersionInputValidator _rowVersionValidatorMock = null!;

    [TestInitialize]
    public void Setup_OkState()
    {
        _rowVersionValidatorMock = Substitute.For<IRowVersionInputValidator>();
        _rowVersionValidatorMock.IsValid(_rowVersion).Returns(true);

        _dut = new UpdateLinkDtoValidator(_rowVersionValidatorMock);
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState()
    {
        // Arrange
        var dto = new UpdateLinkDto("New title", "U", _rowVersion);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenTitleNotGiven()
    {
        // Arrange
        var dto = new UpdateLinkDto(null!, "U", _rowVersion);

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
        var dto = new UpdateLinkDto(
            new string('x', Domain.AggregateModels.LinkAggregate.Link.TitleLengthMax + 1),
            "U",
            _rowVersion);

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
        var dto = new UpdateLinkDto("New title", null!, _rowVersion);

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
        var dto = new UpdateLinkDto(
            "New title",
            new string('x', Domain.AggregateModels.LinkAggregate.Link.UrlLengthMax + 1),
            _rowVersion);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("The length of 'Url' must be"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenRowVersionNotGiven()
    {
        // Arrange
        var dto = new UpdateLinkDto("New title", "U", null!);

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
        var dto = new UpdateLinkDto("New title", "U", _rowVersion);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Dto does not have valid rowVersion!"));
    }
}
