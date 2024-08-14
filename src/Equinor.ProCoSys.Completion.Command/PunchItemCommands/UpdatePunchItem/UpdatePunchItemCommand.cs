using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;

public class UpdatePunchItemCommand(
    Guid punchItemGuid,
    JsonPatchDocument<PatchablePunchItem> patchDocument,
    string rowVersion)
    : IRequest<Result<string>>, IIsPunchItemCommand
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
    public PunchItem PunchItem { get; set; } = null!;
    public JsonPatchDocument<PatchablePunchItem> PatchDocument { get; } = patchDocument;
    public string RowVersion { get; } = rowVersion;
}
