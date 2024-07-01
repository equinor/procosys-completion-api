using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsByCheckList;

public class GetPunchItemsByCheckListGuidQueryHandler : IRequestHandler<GetPunchItemsByCheckListGuidQuery, Result<List<PunchItemDetailsDto>>>
{
    private readonly IPunchItemService _punchItemService;

    public GetPunchItemsByCheckListGuidQueryHandler(IPunchItemService punchItemService) => _punchItemService = punchItemService;
    public async Task<Result<List<PunchItemDetailsDto>>> Handle(GetPunchItemsByCheckListGuidQuery request, CancellationToken cancellationToken)
    {
        var punchItems = await _punchItemService.GetByCheckListGuid(request.CheckListGuid, cancellationToken);
        return new SuccessResult<List<PunchItemDetailsDto>>(punchItems);
    }
}
