using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.WebApi.Controllers;
using Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Controllers.PunchItem;

[TestClass]
public class PatchPunchItemDtoValidatorTests : PatchDtoValidatorTests<PatchPunchItemDto, PatchablePunchItem>
{
    private IRowVersionInputValidator _rowVersionValidatorMock = null!;
    private PatchPunchItemDtoValidator _dut = null!;
    private readonly string _rowVersion = "r";

    protected override void SetupDut()
    {
        _rowVersionValidatorMock = Substitute.For<IRowVersionInputValidator>();
        _rowVersionValidatorMock.IsValid(_rowVersion).Returns(true);
        _dut = new PatchPunchItemDtoValidator(_rowVersionValidatorMock, _patchOperationValidator);
    }

    protected override PatchPunchItemDto GetPatchDto()
    {
        var dto = new PatchPunchItemDto { PatchDocument = new JsonPatchDocument<PatchablePunchItem>() };

        return dto;
    }

    [TestMethod]
    public async Task Validate_ShouldBeValid_WhenRowVersionGiven()
    {
        // Arrange
        var dto = GetPatchDto();
        dto.RowVersion = _rowVersion;

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ShouldFail_WhenRowVersionNotGiven()
    {
        // Arrange
        var dto = GetPatchDto();

        // Act
        var result = await _dut.ValidateAsync(dto);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("'Row Version' must not be empty."));
    }
}
