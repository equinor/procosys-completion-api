using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetCheckListsByPunchItemGuid;

public class GetCheckListsByPunchItemGuidQueryHandler(
    IPlantProvider plantProvider,
    ICheckListApiService checkListApiService)
    : IRequestHandler<GetCheckListsByPunchItemGuidQuery, Result<CheckListsByPunchGuidInstance>>
{
    public async Task<Result<CheckListsByPunchGuidInstance>> Handle(
        GetCheckListsByPunchItemGuidQuery request,
        CancellationToken cancellationToken)
    {
        var res = await checkListApiService.GetByPunchItemGuidAsync(
            plantProvider.Plant, 
            request.PunchItemGuid, 
            cancellationToken);
        return new SuccessResult<CheckListsByPunchGuidInstance>(res);
    }
}

