using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DomainPunchItem = Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate.PunchItem;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Controllers.PunchItem;

[TestClass]
public class PatchPunchItemDtoValidatorTests : PatchDtoValidatorTests<PatchPunchItemDto, PatchablePunchItem>
{
    private PatchPunchItemDtoValidator _dut;

    protected override void SetupDut()
        => _dut = new PatchPunchItemDtoValidator(_rowVersionValidatorMock);

    protected override PatchPunchItemDto GetValidPatchDto()
    {
        var dto = new PatchPunchItemDto { PatchDocument = new JsonPatchDocument<PatchablePunchItem>() };
        dto.PatchDocument.Replace(p => p.RowVersion, RowVersion);

        return dto;
    }

    protected override void AddOperationToPatchDto(PatchPunchItemDto patchDto, OperationType type)
    {
        switch (type)
        {
            case OperationType.Add:
                patchDto.PatchDocument.Add(p => p.Description, "desc");
                break;
            case OperationType.Remove:
                patchDto.PatchDocument.Remove(p => p.Description);
                break;
            case OperationType.Replace:
                patchDto.PatchDocument.Replace(p => p.Description, "desc");
                break;
            case OperationType.Move:
                patchDto.PatchDocument.Move(p1 => p1.Description, p2 => p2.RowVersion);
                break;
            case OperationType.Copy:
                patchDto.PatchDocument.Copy(p1 => p1.Description, p2 => p2.RowVersion);
                break;
            case OperationType.Test:
                patchDto.PatchDocument.Test(p => p.Description, "desc");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenDescriptionIsTooLong()
    {
        // Arrange
        var dto = GetValidPatchDto();
        dto.PatchDocument.Replace(
            p => p.Description,
            new string('x', DomainPunchItem.DescriptionLengthMax + 1));

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("'Description' is required as string and max length is"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenDescriptionIsEmpty()
    {
        // Arrange
        var dto = GetValidPatchDto();
        dto.PatchDocument.Replace(p => p.Description, string.Empty);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("'Description' is required as string and max length is"));
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenDescriptionIsNull()
    {
        // Arrange
        var dto = GetValidPatchDto();
        dto.PatchDocument.Replace(p => p.Description, null);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("'Description' is required as string and max length is"));
    }
}
