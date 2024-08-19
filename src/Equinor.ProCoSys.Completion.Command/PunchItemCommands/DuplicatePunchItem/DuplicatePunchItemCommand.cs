using System;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.DuplicatePunchItem;

public class DuplicatePunchItemCommand(Guid punchItemGuid, Guid[] checkListGuids, bool includeAttachments) : IRequest<Result<string>>
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
    public Guid[] CheckListGuids { get; } = checkListGuids;
    public bool IncludeAttachments { get; } = includeAttachments;
}
