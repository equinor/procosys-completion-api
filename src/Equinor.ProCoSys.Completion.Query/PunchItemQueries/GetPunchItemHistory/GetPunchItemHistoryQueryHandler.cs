using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.History;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemHistory;

public class GetPunchItemHistoryQueryHandler : IRequestHandler<GetPunchItemHistoryQuery, Result<IEnumerable<HistoryDto>>>
{
    private readonly IHistoryService _historyService;

    public GetPunchItemHistoryQueryHandler(IHistoryService commentService) => _historyService = commentService;

    public async Task<Result<IEnumerable<HistoryDto>>> Handle(GetPunchItemHistoryQuery request, CancellationToken cancellationToken)
    {
        var historyDtos = await _historyService.GetAllAsync(request.PunchItemGuid, cancellationToken);
        return new SuccessResult<IEnumerable<HistoryDto>>(historyDtos);
    }
}
