using System;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetCheckListsByPunchItemGuid;

public class GetCheckListsByPunchItemGuidQuery(Guid punchItemGuid)
    : INeedProjectAccess, IRequest<Result<CheckListsByPunchGuidInstance>>, IIsPunchItemRelatedQuery
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
    public ProjectDetailsDto ProjectDetailsDto { get; set; } = null!;
    public Guid GetProjectGuidForAccessCheck() => ProjectDetailsDto.Guid;
}
