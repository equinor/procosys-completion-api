using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.Links;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemLinks;

public class GetPunchItemLinksQueryHandler : IRequestHandler<GetPunchItemLinksQuery, Result<IEnumerable<LinkDto>>>
{
    private readonly ILinkService _linkService;

    public GetPunchItemLinksQueryHandler(ILinkService linkService) => _linkService = linkService;

    public async Task<Result<IEnumerable<LinkDto>>> Handle(GetPunchItemLinksQuery request, CancellationToken cancellationToken)
    {
        var linkDtos = await _linkService.GetAllForSourceAsync(request.PunchItemGuid, cancellationToken);
        return new SuccessResult<IEnumerable<LinkDto>>(linkDtos);
    }
}
