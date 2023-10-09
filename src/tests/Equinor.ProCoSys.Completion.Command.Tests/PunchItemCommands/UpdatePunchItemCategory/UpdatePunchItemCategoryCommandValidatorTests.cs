using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemCategory;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.PunchItemCommands.UpdatePunchItemCategory;

[TestClass]
public class UpdatePunchItemCategoryCommandValidatorTests
{
    private UpdatePunchItemCategoryCommandValidator _dut;
    private IPunchItemValidator _punchItemValidatorMock;
    private UpdatePunchItemCategoryCommand _command;

    [TestInitialize]
    public void Setup_OkState()
    {
        _command = new UpdatePunchItemCategoryCommand(Guid.NewGuid(), Category.PA, "AAAAAAAAABA=");

        _punchItemValidatorMock = Substitute.For<IPunchItemValidator>();
        _punchItemValidatorMock.ExistsAsync(_command.PunchItemGuid, default)
            .Returns(true);
        _punchItemValidatorMock.HasCategoryAsync(_command.PunchItemGuid, _command.Category, default)
            .Returns(false);

        _dut = new UpdatePunchItemCategoryCommandValidator(_punchItemValidatorMock);
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState()
    {
        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_PunchItemNotExists()
    {
        // Arrange
        _punchItemValidatorMock.ExistsAsync(_command.PunchItemGuid, default)
            .Returns(false);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Punch item with this guid does not exist!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_PunchItemIsVoided()
    {
        // Arrange
        _punchItemValidatorMock.TagOwningPunchItemIsVoidedAsync(_command.PunchItemGuid, default)
            .Returns(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Tag owning punch item is voided!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_ProjectIsClosed()
    {
        // Arrange
        _punchItemValidatorMock.ProjectOwningPunchItemIsClosedAsync(_command.PunchItemGuid, default)
            .Returns(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Project is closed!"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_When_PunchItemAlreadyHasCategory()
    {
        // Arrange
        _punchItemValidatorMock.HasCategoryAsync(_command.PunchItemGuid, _command.Category, default)
            .Returns(true);

        // Act
        var result = await _dut.ValidateAsync(_command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"Punch item already have category {_command.Category}!"));
    }
}
