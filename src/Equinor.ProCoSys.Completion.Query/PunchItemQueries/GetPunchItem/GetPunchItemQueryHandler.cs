using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItem;

public class GetPunchItemQueryHandler : IRequestHandler<GetPunchItemQuery, Result<PunchItemDetailsDto>>
{
    private readonly IPunchItemService _punchItemService;

    public GetPunchItemQueryHandler(IPunchItemService punchItemService) => _punchItemService = punchItemService;

    public async Task<Result<PunchItemDetailsDto>> Handle(GetPunchItemQuery request, CancellationToken cancellationToken)
    {
        var punchItem = await _punchItemService.GetByGuid(request.PunchItemGuid, cancellationToken);
        return new SuccessResult<PunchItemDetailsDto>(punchItem);
    }
}
