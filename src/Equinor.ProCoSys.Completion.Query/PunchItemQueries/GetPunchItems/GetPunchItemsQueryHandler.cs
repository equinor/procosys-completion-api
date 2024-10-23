using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItems;

public sealed class GetPunchItemsQueryHandler : IRequestHandler<GetPunchItemsQuery, IEnumerable<PunchItemTinyDetailsDto>>
{
    public Task<IEnumerable<PunchItemTinyDetailsDto>> Handle(GetPunchItemsQuery request, CancellationToken cancellationToken)
    {
        // we want to use MediatR pipeline to handle the request even if the handler do nothing
        // the reason is that MediatR pipeline performs access check
        var result = request.PunchItemsDetailsDto;
        return Task.FromResult(result);
    }
}
