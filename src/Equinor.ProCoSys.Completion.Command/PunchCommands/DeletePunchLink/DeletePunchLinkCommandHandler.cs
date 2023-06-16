using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Links;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.DeletePunchLink;

public class DeletePunchLinkCommandHandler : IRequestHandler<DeletePunchLinkCommand, Result<Unit>>
{
    private readonly ILinkService _linkService;

    public DeletePunchLinkCommandHandler(ILinkService linkService) => _linkService = linkService;

    public async Task<Result<Unit>> Handle(DeletePunchLinkCommand request, CancellationToken cancellationToken)
    {
        await _linkService.DeleteAsync(
            request.LinkGuid,
            request.RowVersion,
            cancellationToken);

        return new SuccessResult<Unit>(Unit.Value);
    }
}
