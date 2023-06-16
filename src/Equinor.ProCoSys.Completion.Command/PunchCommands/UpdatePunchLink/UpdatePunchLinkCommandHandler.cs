using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Links;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.UpdatePunchLink;

public class UpdatePunchLinkCommandHandler : IRequestHandler<UpdatePunchLinkCommand, Result<string>>
{
    private readonly ILinkService _linkService;

    public UpdatePunchLinkCommandHandler(ILinkService linkService) => _linkService = linkService;

    public async Task<Result<string>> Handle(UpdatePunchLinkCommand request, CancellationToken cancellationToken)
    {
        var rowVersion = await _linkService.UpdateAsync(
            request.LinkGuid,
            request.Title,
            request.Url,
            request.RowVersion,
            cancellationToken);

        return new SuccessResult<string>(rowVersion);
    }
}
