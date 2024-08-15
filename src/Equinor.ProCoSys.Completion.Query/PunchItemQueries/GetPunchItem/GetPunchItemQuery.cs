using System;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItem;

public class GetPunchItemQuery(Guid punchItemGuid)
    : NeedProjectAccess, IRequest<Result<PunchItemDetailsDto>>, IIsPunchItemQuery
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
    public PunchItemDetailsDto PunchItemDetailsDto { get; set; } = null!;
    public override Guid GetProjectGuidForAccessCheck() => PunchItemDetailsDto.ProjectGuid;
}
