using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsByCheckList;

public class GetPunchItemsByCheckListGuidQuery(Guid checkListGuid)
    : INeedProjectAccess, IRequest<Result<IEnumerable<PunchItemDetailsDto>>>, IIsCheckListQuery
{
    public Guid CheckListGuid { get; } = checkListGuid;
    public CheckListDetailsDto CheckListDetailsDto { get; set; } = null!;
    public Guid GetProjectGuidForAccessCheck() => CheckListDetailsDto.ProjectGuid;
}
