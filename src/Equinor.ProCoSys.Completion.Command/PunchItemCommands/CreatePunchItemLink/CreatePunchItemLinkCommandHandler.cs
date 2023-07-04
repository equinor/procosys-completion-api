using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Links;
using MediatR;
using ServiceResult;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemLink;

public class CreatePunchItemLinkCommandHandler : IRequestHandler<CreatePunchItemLinkCommand, Result<GuidAndRowVersion>>
{
    private readonly ILinkService _linkService;

    public CreatePunchItemLinkCommandHandler(ILinkService linkService) => _linkService = linkService;

    public async Task<Result<GuidAndRowVersion>> Handle(CreatePunchItemLinkCommand request, CancellationToken cancellationToken)
    {
        var linkDto = await _linkService.AddAsync(nameof(PunchItem), request.PunchItemGuid, request.Title, request.Url, cancellationToken);

        return new SuccessResult<GuidAndRowVersion>(new GuidAndRowVersion(linkDto.Guid, linkDto.RowVersion));
    }
}
