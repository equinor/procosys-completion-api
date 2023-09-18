using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Controllers.PunchItem;

[TestClass]
public class PatchPunchItemDtoValidatorTests : PatchDtoValidatorTests<PatchPunchItemDto, PatchablePunchItem>
{
    protected override PatchPunchItemDto GetPatchDto()
    {
        var dto = new PatchPunchItemDto { PatchDocument = new JsonPatchDocument<PatchablePunchItem>() };

        return dto;
    }
}
