using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.WebApi.Controllers;
using Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Controllers.PunchItem;

[TestClass]
public class RejectPunchItemDtoValidatorTests
{
    private readonly string _rowVersion = "AAAAAAAAABA=";

    private RejectPunchItemDtoValidator _dut = null!;
    private IRowVersionInputValidator _rowVersionValidatorMock = null!;

    [TestInitialize]
    public void Setup_OkState()
    {
        _rowVersionValidatorMock = Substitute.For<IRowVersionInputValidator>();
        _rowVersionValidatorMock.IsValid(_rowVersion).Returns(true);

        _dut = new RejectPunchItemDtoValidator(_rowVersionValidatorMock);
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState()
    {
        // Arrange
        var dto = new RejectPunchItemDto("Rejected because", _rowVersion);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenCommentNotGiven()
    {
        // Arrange
        var dto = new RejectPunchItemDto(null!, _rowVersion);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("'Comment' must not be empty."));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenCommentIsEmpty()
    {
        // Arrange
        var dto = new RejectPunchItemDto(string.Empty, _rowVersion);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("The length of 'Comment' must be at least"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenCommentIsTooLongAsync()
    {
        // Arrange
        var dto = new RejectPunchItemDto(
            new string('x', Domain.AggregateModels.CommentAggregate.Comment.TextLengthMax + 1),
            _rowVersion);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("The length of 'Comment' must be"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenRowVersionNotGiven()
    {
        // Arrange
        var dto = new RejectPunchItemDto("Rejected because", null!);

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
        var dto = new RejectPunchItemDto("Rejected because", _rowVersion);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Dto does not have valid rowVersion!"));
    }
}
