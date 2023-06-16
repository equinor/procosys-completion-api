using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.Links;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchLinks;

public class GetPunchLinksQueryHandler : IRequestHandler<GetPunchLinksQuery, Result<IEnumerable<LinkDto>>>
{
    private readonly ILinkService _linkService;

    public GetPunchLinksQueryHandler(ILinkService linkService) => _linkService = linkService;

    public async Task<Result<IEnumerable<LinkDto>>> Handle(GetPunchLinksQuery request, CancellationToken cancellationToken)
    {
        var linkDtos = await _linkService.GetAllForSourceAsync(request.PunchGuid, cancellationToken);
        return new SuccessResult<IEnumerable<LinkDto>>(linkDtos);
    }
}
