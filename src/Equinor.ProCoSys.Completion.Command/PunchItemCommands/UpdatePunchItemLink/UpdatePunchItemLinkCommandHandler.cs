using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Links;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemLink;

public class UpdatePunchItemLinkCommandHandler : IRequestHandler<UpdatePunchItemLinkCommand, Result<string>>
{
    private readonly ILinkService _linkService;

    public UpdatePunchItemLinkCommandHandler(ILinkService linkService) => _linkService = linkService;

    public async Task<Result<string>> Handle(UpdatePunchItemLinkCommand request, CancellationToken cancellationToken)
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
