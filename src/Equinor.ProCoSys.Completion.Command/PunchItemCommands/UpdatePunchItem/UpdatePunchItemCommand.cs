using System;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;

public class UpdatePunchItemCommand : IRequest<Result<string>>, IIsPunchItemCommand
{
    public UpdatePunchItemCommand(Guid punchItemGuid, JsonPatchDocument<PatchablePunchItem> patchDocument/*, string rowVersion*/)
    {
        PunchItemGuid = punchItemGuid;
        PatchDocument = patchDocument;
        //RowVersion = rowVersion;
    }

    public Guid PunchItemGuid { get; }
    public JsonPatchDocument<PatchablePunchItem> PatchDocument { get; }
    //public string RowVersion { get; }
}
