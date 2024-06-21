using System;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.GetCheckListsByPunchItemGuid;

public class GetCheckListsByPIGuidCommand : IRequest<Result<ChecklistsByPunchGuidInstance>>
{
    public GetCheckListsByPIGuidCommand(Guid punchItemGuid) => PunchItemGuid = punchItemGuid;

    public Guid PunchItemGuid { get; }
}
