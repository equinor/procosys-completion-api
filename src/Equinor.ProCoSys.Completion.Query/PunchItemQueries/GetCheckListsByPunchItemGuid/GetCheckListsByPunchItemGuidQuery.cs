using System;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetCheckListsByPunchItemGuid;

public class GetCheckListsByPunchItemGuidQuery(Guid punchItemGuid) : IRequest<Result<ChecklistsByPunchGuidInstance>>, IIsPunchItemQuery
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
}
