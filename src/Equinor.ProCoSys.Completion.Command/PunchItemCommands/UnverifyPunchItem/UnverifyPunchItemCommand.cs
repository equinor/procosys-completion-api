using System;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnverifyPunchItem;

public class UnverifyPunchItemCommand(Guid punchItemGuid, string rowVersion)
    : CanHaveCheckListRestrictionsViaCheckList, IRequest<Result<string>>, IIsPunchItemCommand
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
    public PunchItem PunchItem { get; set; } = null!;
    public override Guid GetProjectGuidForAccessCheck() => PunchItem.Project.Guid;
    public override Guid GetCheckListGuidForWriteAccessCheck() => PunchItem.CheckListGuid;
    public string RowVersion { get; } = rowVersion;
}
