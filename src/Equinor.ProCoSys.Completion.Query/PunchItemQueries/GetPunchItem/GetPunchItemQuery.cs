using System;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItem;

public class GetPunchItemQuery(Guid punchItemGuid)
    : INeedProjectAccess, IRequest<PunchItemDetailsDto>, IIsPunchItemQuery
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
    public PunchItemDetailsDto PunchItemDetailsDto { get; set; } = null!;
    public Guid GetProjectGuidForAccessCheck() => PunchItemDetailsDto.ProjectGuid;
}
