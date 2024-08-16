using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnverifyPunchItem;

public class UnverifyPunchItemCommand(Guid punchItemGuid, string rowVersion)
    : ICanHaveRestrictionsViaCheckList, IRequest<Result<string>>, IIsPunchItemCommand
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
    public PunchItem PunchItem { get; set; } = null!;
    public Guid GetProjectGuidForAccessCheck() => PunchItem.Project.Guid;
    public Guid GetCheckListGuidForWriteAccessCheck() => PunchItem.CheckListGuid;
    public string RowVersion { get; } = rowVersion;
}
