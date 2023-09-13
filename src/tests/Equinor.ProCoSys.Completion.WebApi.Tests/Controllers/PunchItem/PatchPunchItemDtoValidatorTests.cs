using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DomainPunchItem = Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate.PunchItem;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Controllers.PunchItem;

[TestClass]
public class PatchPunchItemDtoValidatorTests : PatchDtoValidatorTests<PatchPunchItemDto>
{
    private PatchPunchItemDtoValidator _dut;

    protected override void SetupDut()
        => _dut = new PatchPunchItemDtoValidator(_rowVersionValidatorMock);

    protected override PatchPunchItemDto GetValidPatchDto()
    {
        var dto = new PatchPunchItemDto { PatchDocument = new JsonPatchDocument() };
        dto.PatchDocument.Replace($"/{nameof(DomainPunchItem.RowVersion)}", RowVersion);

        return dto;
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenDescriptionIsTooLong()
    {
        // Arrange
        var dto = GetValidPatchDto();
        dto.PatchDocument.Replace($"/{nameof(DomainPunchItem.Description)}",
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
        dto.PatchDocument.Replace($"/{nameof(DomainPunchItem.Description)}", string.Empty);

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
        dto.PatchDocument.Replace($"/{nameof(DomainPunchItem.Description)}", null);

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("'Description' is required as string and max length is"));
    }
}
