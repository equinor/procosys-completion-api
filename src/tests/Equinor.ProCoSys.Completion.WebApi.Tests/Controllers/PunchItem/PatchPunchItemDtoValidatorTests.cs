using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DomainPunchItem = Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate.PunchItem;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Controllers.PunchItem;

[TestClass]
public class PatchPunchItemDtoValidatorTests : PatchDtoValidatorTests<PatchPunchItemDto, PatchablePunchItem>
{
    private PatchPunchItemDtoValidator _dut = null!;
    //private readonly string _rowVersion = "r";

    protected override void SetupDut()
        => _dut = new PatchPunchItemDtoValidator(_patchOperationValidator);

    protected override PatchPunchItemDto GetPatchDto()
    {
        var dto = new PatchPunchItemDto { PatchDocument = new JsonPatchDocument<PatchablePunchItem>() };
        //dto.PatchDocument.Replace(p => p.RowVersion, _rowVersion);

        return dto;
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenDescriptionIsTooLong()
    {
        // Arrange
        var dto = GetPatchDto();
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
        var dto = GetPatchDto();
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
        var dto = GetPatchDto();
        dto.PatchDocument.Replace(p => p.Description, null);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("'Description' is required as string and max length is"));
    }
}
