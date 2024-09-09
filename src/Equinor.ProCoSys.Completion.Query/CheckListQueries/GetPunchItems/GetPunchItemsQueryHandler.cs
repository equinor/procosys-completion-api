using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.CheckListQueries.GetPunchItems;

public class GetPunchItemsQueryHandler(IPunchItemService punchItemService)
    : IRequestHandler<GetPunchItemsQuery, IEnumerable<PunchItemDetailsDto>>
{
    public async Task<IEnumerable<PunchItemDetailsDto>> Handle(GetPunchItemsQuery request, CancellationToken cancellationToken)
    {
        var punchItems = await punchItemService.GetByCheckListGuid(request.CheckListGuid, cancellationToken);
        return punchItems;
    }
}
