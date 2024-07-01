using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsByCheckList;

public class GetPunchItemsByCheckListGuidQuery(Guid checkListGuid) : IRequest<Result<List<PunchItemDetailsDto>>>, IIsCheckListQuery
{
    public Guid CheckListGuid { get; } = checkListGuid;
}
