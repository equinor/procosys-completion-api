using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.ProjectQueries.SearchCheckLists;

public class SearchCheckListsQueryHandler(ICheckListApiService checkListService)
    : IRequestHandler<SearchCheckListsQuery, SearchResultDto>
{
    public async Task<SearchResultDto> Handle(
        SearchCheckListsQuery request, 
        CancellationToken cancellationToken)
    {
        string? tagFunctionCode = null;
        string? tagRegisterCode = null;

        if (request.RegisterAndTagFunctionCode is not null)
        {
            var codeParts = request.RegisterAndTagFunctionCode?.Split("/")!;
            tagRegisterCode = codeParts[0];
            tagFunctionCode = codeParts[1];
        }
        var pcs4SearchResult = await checkListService.SearchCheckListsAsync(
            request.ProjectGuid,
            request.MultipleTagNo,
            request.TagNoContains,
            request.ResponsibleCode,
            tagRegisterCode,
            tagFunctionCode,
            request.FormularType,
            request.CurrentPage,
            request.ItemsPerPage,
            cancellationToken);

        var searchResult = new SearchResultDto(
            pcs4SearchResult.MaxAvailable,
            pcs4SearchResult.Items.Select(c => new CheckListDto(c)));
        return searchResult;
    }
}
