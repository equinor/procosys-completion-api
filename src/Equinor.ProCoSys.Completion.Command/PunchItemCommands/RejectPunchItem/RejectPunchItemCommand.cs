using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.RejectPunchItem;

public class RejectPunchItemCommand(
    Guid punchItemGuid,
    string comment,
    IEnumerable<Guid> mentions,
    string rowVersion)
    : ICanHaveRestrictionsViaCheckList, IRequest<Result<string>>, IIsPunchItemCommand
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
    public PunchItem PunchItem { get; set; } = null!;
    public Guid GetProjectGuidForAccessCheck() => PunchItem.Project.Guid;
    public Guid GetCheckListGuidForWriteAccessCheck() => PunchItem.CheckListGuid;
    public string Comment { get; } = comment;
    public IEnumerable<Guid> Mentions { get; } = mentions;
    public string RowVersion { get; } = rowVersion;
}
