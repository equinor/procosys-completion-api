using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItem;

public readonly record struct GetPunchItemQuery(Guid PunchItemGuid)
    : IRequest<Result<PunchItemDetailsDto>>, IIsPunchItemQuery;

public sealed class GetPunchItemQueryHandler(IPunchItemService punchItemService)
    : IRequestHandler<GetPunchItemQuery, Result<PunchItemDetailsDto>>
{
    public async Task<Result<PunchItemDetailsDto>> Handle(GetPunchItemQuery request,
        CancellationToken cancellationToken)
    {
        var punchItem = await punchItemService.GetByGuid(request.PunchItemGuid, cancellationToken);
        if (punchItem is null)
        {
            return new NotFoundResult<PunchItemDetailsDto>($"Could not find {nameof(PunchItem)} with Guid '{request.PunchItemGuid}'");
        }

        return new SuccessResult<PunchItemDetailsDto>(punchItem);
    }
}
