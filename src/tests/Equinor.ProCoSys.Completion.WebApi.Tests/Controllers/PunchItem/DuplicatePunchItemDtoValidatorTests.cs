using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Controllers.PunchItem;

[TestClass]
public class DuplicatePunchItemDtoValidatorTests
{
    private DuplicatePunchItemDtoValidator _dut = null!;
    private readonly IOptionsMonitor<ApplicationOptions> _applicationOptionsMock
        = Substitute.For<IOptionsMonitor<ApplicationOptions>>();
    private readonly int _maxDuplicatePunch = 3;

    [TestInitialize]
    public void Setup_OkState()
    {
        _applicationOptionsMock.CurrentValue.Returns(new ApplicationOptions { MaxDuplicatePunch = _maxDuplicatePunch });

        _dut = new(_applicationOptionsMock);
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState()
    {
        // Arrange
        var dto = new DuplicatePunchItemDto([Guid.NewGuid(), Guid.NewGuid()], false);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenCheckListsNotGiven()
    {
        // Arrange
        var dto = new DuplicatePunchItemDto([], false);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"Number of check lists to duplicate to must be between 1 and {_maxDuplicatePunch}"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenToManyCheckListsGiven()
    {
        // Arrange
        var dto = new DuplicatePunchItemDto([Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()], false);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"Number of check lists to duplicate to must be between 1 and {_maxDuplicatePunch}"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenCheckListNotUnique()
    {
        // Arrange
        var newGuid = Guid.NewGuid();
        var dto = new DuplicatePunchItemDto([newGuid, newGuid, newGuid], false);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Check lists must be unique"));
    }
}
