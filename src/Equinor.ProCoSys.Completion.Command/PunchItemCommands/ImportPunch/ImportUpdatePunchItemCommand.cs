using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportPunch;

public sealed record ImportUpdatePunchItemCommand(
    Guid ImportGuid,
    Guid ProjectGuid,
    string Plant,
    Guid PunchItemGuid,
    JsonPatchDocument<PatchablePunchItem> PatchDocument,
    Category? Category,
    Optional<ActionByPerson?> ClearedBy,
    Optional<ActionByPerson?> VerifiedBy,
    Optional<ActionByPerson?> RejectedBy,
    string RowVersion) : ICanHaveRestrictionsViaCheckList, IRequest<List<ImportError>>, IIsPunchItemCommand , IImportCommand
{
    public PunchItem PunchItem { get; set; } = null!;
    public Guid GetProjectGuidForAccessCheck() => PunchItem.Project.Guid;
    public Guid GetCheckListGuidForWriteAccessCheck() => PunchItem.CheckListGuid;
    public CheckListDetailsDto CheckListDetailsDto { get; set; } = null!;
}
