using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.Responsibles;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.TagFunctions;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.CheckListQueries.GetDuplicateInfo;

public class GetDuplicateInfoQueryHandler(
    IPlantProvider plantProvider, 
    IResponsibleApiService responsibleService,
    ITagFunctionApiService tagFunctionService)
    : IRequestHandler<GetDuplicateInfoQuery, Result<DuplicateInfoDto>>
{
    public async Task<Result<DuplicateInfoDto>> Handle(GetDuplicateInfoQuery request, CancellationToken cancellationToken)
    {
        var checkList = request.CheckListDetailsDto;

        // run in parallel
        var responsiblesTask = responsibleService.GetAllAsync(plantProvider.Plant, cancellationToken);
        var tagFunctionsTask = tagFunctionService.GetAllAsync(plantProvider.Plant, cancellationToken);
        await Task.WhenAll(responsiblesTask, tagFunctionsTask);

        var responsibles = await responsiblesTask;
        var tagFunctions = await tagFunctionsTask;

        var responsibleDtos = responsibles.Select(r => new ResponsibleDto(r.Code, r.Description));
        var tagFunctionDtos = tagFunctions.Select(tf => new TagFunctionDto(tf.ToString(), tf.Description));
        
        DuplicateInfoDto result = new(checkList, responsibleDtos, tagFunctionDtos);
        return new SuccessResult<DuplicateInfoDto>(result);
    }
}
