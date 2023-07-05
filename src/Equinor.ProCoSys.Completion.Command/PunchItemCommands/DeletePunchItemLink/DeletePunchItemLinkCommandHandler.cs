using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Links;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItemLink;

public class DeletePunchItemLinkCommandHandler : IRequestHandler<DeletePunchItemLinkCommand, Result<Unit>>
{
    private readonly ILinkService _linkService;

    public DeletePunchItemLinkCommandHandler(ILinkService linkService) => _linkService = linkService;

    public async Task<Result<Unit>> Handle(DeletePunchItemLinkCommand request, CancellationToken cancellationToken)
    {
        await _linkService.DeleteAsync(
            request.LinkGuid,
            request.RowVersion,
            cancellationToken);

        return new SuccessResult<Unit>(Unit.Value);
    }
}
