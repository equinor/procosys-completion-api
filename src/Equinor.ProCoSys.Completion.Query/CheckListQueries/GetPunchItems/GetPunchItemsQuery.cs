using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.CheckListQueries.GetPunchItems;

public class GetPunchItemsQuery(Guid checkListGuid)
    : IRequest<Result<IEnumerable<PunchItemDetailsDto>>>, IIsCheckListQuery
{
    public Guid CheckListGuid { get; } = checkListGuid;
    public CheckListDetailsDto CheckListDetailsDto { get; set; } = null!;
    public Guid GetProjectGuidForAccessCheck() => CheckListDetailsDto.ProjectGuid;
}
