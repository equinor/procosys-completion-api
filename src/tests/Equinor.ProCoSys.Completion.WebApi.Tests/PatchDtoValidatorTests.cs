using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.WebApi.Controllers;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests;

[TestClass]
public abstract class PatchDtoValidatorTests<T1, T2> where T1 : PatchDto<T2> where T2: class, new()
{
    protected readonly string RowVersion = "AAAAAAAAABA=";
    private PatchDtoValidator<T1, T2> _dut;
    protected IRowVersionValidator _rowVersionValidatorMock;

    protected abstract void SetupDut();
    protected abstract T1 GetValidPatchDto();
    protected abstract void AddOperationToPatchDto(T1 patchDto, OperationType type);

    [TestInitialize]
    public void Setup_OkState()
    {
        _rowVersionValidatorMock = Substitute.For<IRowVersionValidator>();
        _rowVersionValidatorMock.IsValid(RowVersion).Returns(true);

        _dut = new PatchDtoValidator<T1, T2>(_rowVersionValidatorMock);

        SetupDut();
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenOkState()
    {
        // Act
        var result = await _dut.ValidateAsync(GetValidPatchDto());

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenAddOperationGiven()
    {
        // Arrange
        var patchDto = GetValidPatchDto();
        AddOperationToPatchDto(patchDto, OperationType.Add);

        // Act
        var result = await _dut.ValidateAsync(patchDto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Only 'Replace' operations are supported when patching"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenMoveOperationGiven()
    {
        // Arrange
        var patchDto = GetValidPatchDto();
        AddOperationToPatchDto(patchDto, OperationType.Move);

        // Act
        var result = await _dut.ValidateAsync(patchDto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Only 'Replace' operations are supported when patching"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenCopyOperationGiven()
    {
        // Arrange
        var patchDto = GetValidPatchDto();
        AddOperationToPatchDto(patchDto, OperationType.Copy);

        // Act
        var result = await _dut.ValidateAsync(patchDto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Only 'Replace' operations are supported when patching"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenTestOperationGiven()
    {
        // Arrange
        var patchDto = GetValidPatchDto();
        AddOperationToPatchDto(patchDto, OperationType.Test);

        // Act
        var result = await _dut.ValidateAsync(patchDto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Only 'Replace' operations are supported when patching"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenRemoveOperationGiven()
    {
        // Arrange
        var patchDto = GetValidPatchDto();
        AddOperationToPatchDto(patchDto, OperationType.Remove);

        // Act
        var result = await _dut.ValidateAsync(patchDto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Only 'Replace' operations are supported when patching"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenDuplicateReplaceOperationGiven()
    {
        // Arrange
        var patchDto = GetValidPatchDto();
        AddOperationToPatchDto(patchDto, OperationType.Replace);
        AddOperationToPatchDto(patchDto, OperationType.Replace);

        // Act
        var result = await _dut.ValidateAsync(patchDto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("All operation paths must be unique"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenRowVersionNotGiven()
    {
        // Arrange
        var patchDto = GetValidPatchDto();
        patchDto.PatchDocument.Operations.Clear();

        // Act
        var result = await _dut.ValidateAsync(patchDto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("'RowVersion' is required and must be a valid row version"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenIllegalRowVersion()
    {
        // Arrange
        _rowVersionValidatorMock.IsValid(RowVersion).Returns(false);

        // Act
        var result = await _dut.ValidateAsync(GetValidPatchDto());

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("'RowVersion' is required and must be a valid row version"));
    }
}
