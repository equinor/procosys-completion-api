using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Links;
using MediatR;
using ServiceResult;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.CreatePunchLink;

public class CreatePunchLinkCommandHandler : IRequestHandler<CreatePunchLinkCommand, Result<GuidAndRowVersion>>
{
    private readonly ILinkService _linkService;

    public CreatePunchLinkCommandHandler(ILinkService linkService) => _linkService = linkService;

    public async Task<Result<GuidAndRowVersion>> Handle(CreatePunchLinkCommand request, CancellationToken cancellationToken)
    {
        var linkDto = await _linkService.AddAsync(nameof(Punch), request.PunchGuid, request.Title, request.Url, cancellationToken);

        return new SuccessResult<GuidAndRowVersion>(new GuidAndRowVersion(linkDto.Guid, linkDto.RowVersion));
    }
}
