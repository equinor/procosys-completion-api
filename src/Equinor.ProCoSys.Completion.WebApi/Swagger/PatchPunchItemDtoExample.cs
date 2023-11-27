using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;
using Microsoft.AspNetCore.JsonPatch;
using Swashbuckle.AspNetCore.Filters;

namespace Equinor.ProCoSys.Completion.WebApi.Swagger;

public class PatchPunchItemDtoExample : IExamplesProvider<PatchPunchItemDto>
{
    public PatchPunchItemDto GetExamples()
    {
        var patchDocument = new JsonPatchDocument<PatchablePunchItem>();
        var patchPunchItemDto = new PatchPunchItemDto
        {
            RowVersion = "AAAAAAAAABA=", 
            PatchDocument = patchDocument
        };
        SwaggerDocHelper.FillPatchDocumentWithSampleData(patchDocument);
        return patchPunchItemDto;
    }
}
