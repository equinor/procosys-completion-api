using System;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;

public class UpdatePunchItemCommand : IRequest<Result<string>>, IIsPunchItemCommand
{
    public UpdatePunchItemCommand(Guid punchItemGuid, JsonPatchDocument<PatchablePunchItem> patchDocument)
    {
        PunchItemGuid = punchItemGuid;
        PatchDocument = patchDocument;
    }

    public Guid PunchItemGuid { get; }
    public JsonPatchDocument<PatchablePunchItem> PatchDocument { get; }
}
