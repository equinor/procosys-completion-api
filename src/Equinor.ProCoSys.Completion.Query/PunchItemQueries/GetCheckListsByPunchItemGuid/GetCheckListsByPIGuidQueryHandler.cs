using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetCheckListsByPunchItemGuid;

public class GetCheckListsByPIGuidQueryHandler : IRequestHandler<GetCheckListsByPIGuidQuery, Result<ChecklistsByPunchGuidInstance>>
{
    private readonly ICheckListApiService _checkListApiService;
    private readonly IPlantProvider _plantProvider;

    public GetCheckListsByPIGuidQueryHandler(
        IPlantProvider plantProvider,
        ICheckListApiService checkListApiService
        )
    {
        _plantProvider = plantProvider;
        _checkListApiService = checkListApiService;
    }

    public async Task<Result<ChecklistsByPunchGuidInstance>> Handle(GetCheckListsByPIGuidQuery request,
    CancellationToken cancellationToken)
    {
        var res = await _checkListApiService.GetByPunchItemGuidAsync(_plantProvider.Plant, request.PunchItemGuid);
        return new SuccessResult<ChecklistsByPunchGuidInstance>(res!);
    }
}

