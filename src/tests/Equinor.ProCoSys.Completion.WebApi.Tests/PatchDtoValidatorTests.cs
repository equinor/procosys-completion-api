using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.WebApi.Controllers;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests;

[TestClass]
public abstract class PatchDtoValidatorTests<T1, T2> where T1 : PatchDto<T2> where T2: class
{
    private PatchDtoValidator<T1, T2> _dut = null!;
    protected IPatchOperationInputValidator _patchOperationValidator = null!;

    protected abstract void SetupDut();
    protected abstract T1 GetPatchDto();

    [TestInitialize]
    public void Setup_OkState()
    {
        _patchOperationValidator = Substitute.For<IPatchOperationInputValidator>();
        _patchOperationValidator.HaveUniqueReplaceOperations(Arg.Any<List<Operation<T2>>>()).Returns(true);
        _patchOperationValidator.HaveReplaceOperationsOnly(Arg.Any<List<Operation<T2>>>()).Returns(true);
        _patchOperationValidator.AllRequiredFieldsHaveValue(Arg.Any<List<Operation<T2>>>()).Returns(true);
        _patchOperationValidator.HaveValidReplaceOperationsOnly(Arg.Any<List<Operation<T2>>>()).Returns(true);
        _patchOperationValidator.HaveValidLengthOfStrings(Arg.Any<List<Operation<T2>>>()).Returns(true);

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
    public async Task Validate_ShouldFail_WhenRequiredFieldGivenWithoutValue()
    {
        // Arrange
        _patchOperationValidator.AllRequiredFieldsHaveValue(Arg.Any<List<Operation<T2>>>()).Returns(false);
        var message = "message with info";
        _patchOperationValidator.GetMessageForRequiredFields(Arg.Any<List<Operation<T2>>>()).Returns(message);

        // Act
        var result = await _dut.ValidateAsync(GetPatchDto());

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.Equals(message));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenInvalidOperationExist()
    {
        // Arrange
        _patchOperationValidator.HaveValidReplaceOperationsOnly(Arg.Any<List<Operation<T2>>>()).Returns(false);
        var message = "message with info";
        _patchOperationValidator.GetMessageForInvalidReplaceOperations(
            Arg.Any<List<Operation<T2>>>()).Returns(message);

        // Act
        var result = await _dut.ValidateAsync(GetPatchDto());

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.Equals(message));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenInvalidStringGiven()
    {
        // Arrange
        _patchOperationValidator.HaveValidLengthOfStrings(Arg.Any<List<Operation<T2>>>()).Returns(false);
        var message = "message with info";
        _patchOperationValidator.GetMessageForInvalidLengthOfStrings(Arg.Any<List<Operation<T2>>>()).Returns(message);

        // Act
        var result = await _dut.ValidateAsync(GetPatchDto());

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.Equals(message));
    }
}
