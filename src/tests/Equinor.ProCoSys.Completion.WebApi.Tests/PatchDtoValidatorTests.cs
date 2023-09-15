using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.WebApi.Controllers;
using Equinor.ProCoSys.Completion.WebApi.InputValidators;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests;

[TestClass]
public abstract class PatchDtoValidatorTests<T1, T2> where T1 : PatchDto<T2> where T2: class
{
    private PatchDtoValidator<T1, T2> _dut = null!;
    protected IPatchOperationValidator _patchOperationValidator = null!;

    protected abstract void SetupDut();
    protected abstract T1 GetPatchDto();

    [TestInitialize]
    public void Setup_OkState()
    {
        _patchOperationValidator = Substitute.For<IPatchOperationValidator>();
        _patchOperationValidator.HaveUniqueReplaceOperations(Arg.Any<List<Operation<T2>>>()).Returns(true);
        _patchOperationValidator.HaveReplaceOperationsOnly(Arg.Any<List<Operation<T2>>>()).Returns(true);
        _patchOperationValidator.HaveValidReplaceOperationsOnly(Arg.Any<List<Operation<T2>>>()).Returns(true);
        _patchOperationValidator.HaveValidRowVersionOperation(Arg.Any<List<Operation<T2>>>()).Returns(true);

        _dut = new PatchDtoValidator<T1, T2>(_patchOperationValidator);

        SetupDut();
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState()
    {
        // Act
        var result = await _dut.ValidateAsync(GetPatchDto());

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenDuplicatesGiven()
    {
        // Arrange
        _patchOperationValidator.HaveUniqueReplaceOperations(Arg.Any<List<Operation<T2>>>()).Returns(false);

        // Act
        var result = await _dut.ValidateAsync(GetPatchDto());

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("All operation paths must be unique"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenOtherThanReplaceGiven()
    {
        // Arrange
        _patchOperationValidator.HaveReplaceOperationsOnly(Arg.Any<List<Operation<T2>>>()).Returns(false);

        // Act
        var result = await _dut.ValidateAsync(GetPatchDto());

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Only 'Replace' operations are supported when patching"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenRowVersionNotGiven()
    {
        // Arrange
        _patchOperationValidator.HaveValidRowVersionOperation(Arg.Any<List<Operation<T2>>>()).Returns(false);

        // Act
        var result = await _dut.ValidateAsync(GetPatchDto());

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("'RowVersion' is required and must be a valid row version"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenIllegalOperationExist()
    {
        // Arrange
        _patchOperationValidator.HaveValidReplaceOperationsOnly(
            Arg.Any<List<Operation<T2>>>()).Returns(false);
        var message = "message with info";
        _patchOperationValidator.GetMessageForIllegalReplaceOperations(
            Arg.Any<List<Operation<T2>>>()).Returns(message);

        // Act
        var result = await _dut.ValidateAsync(GetPatchDto());

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.Equals(message));
    }
}
