using System;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetCheckListsByPunchItemGuid;

public class GetCheckListsByPIGuidQuery(Guid punchItemGuid) : IRequest<Result<ChecklistsByPunchGuidInstance>>
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
}
