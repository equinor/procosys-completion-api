using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItem;

public sealed class GetPunchItemQueryHandler : IRequestHandler<GetPunchItemQuery, PunchItemDetailsDto>
{
    public Task<PunchItemDetailsDto> Handle(GetPunchItemQuery request, CancellationToken cancellationToken)
    {
        // we want to use MediatR pipeline to handle the request even if the handler do nothing
        // the reason is that MediatR pipeline performs access check
        var result = request.PunchItemDetailsDto;
        return Task.FromResult(result);
    }
}
