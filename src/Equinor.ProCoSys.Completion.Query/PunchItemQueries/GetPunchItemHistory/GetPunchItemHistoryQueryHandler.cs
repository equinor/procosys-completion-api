using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.History;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemHistory;

public class GetPunchItemHistoryQueryHandler : IRequestHandler<GetPunchItemHistoryQuery, IEnumerable<HistoryDto>>
{
    private readonly IHistoryService _historyService;

    public GetPunchItemHistoryQueryHandler(IHistoryService commentService) => _historyService = commentService;

    public async Task<IEnumerable<HistoryDto>> Handle(GetPunchItemHistoryQuery request, CancellationToken cancellationToken)
    {
        var historyDtos = await _historyService.GetAllAsync(request.PunchItemGuid, cancellationToken);
        return historyDtos;
    }
}
