using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsByCheckList;

public readonly record struct GetPunchItemsByCheckListGuidQuery(Guid CheckListGuid, PunchListStatusFilter StatusFilter)
    : IRequest<Result<IEnumerable<PunchItemDetailsDto>>>, IIsCheckListQuery;

public sealed class GetPunchItemsByCheckListGuidQueryHandler(IPunchItemService punchItemService)
    : IRequestHandler<GetPunchItemsByCheckListGuidQuery, Result<IEnumerable<PunchItemDetailsDto>>>
{
    public async Task<Result<IEnumerable<PunchItemDetailsDto>>> Handle(GetPunchItemsByCheckListGuidQuery request,
        CancellationToken cancellationToken)
        => new SuccessResult<IEnumerable<PunchItemDetailsDto>>(
            await punchItemService.GetByCheckListGuidAsync(request.CheckListGuid, request.StatusFilter, cancellationToken));
}
