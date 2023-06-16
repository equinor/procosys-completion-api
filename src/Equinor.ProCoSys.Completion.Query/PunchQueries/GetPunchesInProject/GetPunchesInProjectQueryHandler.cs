using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchesInProject;

public class GetPunchesInProjectQueryHandler : IRequestHandler<GetPunchesInProjectQuery, Result<IEnumerable<PunchDto>>>
{
    private readonly IReadOnlyContext _context;

    public GetPunchesInProjectQueryHandler(IReadOnlyContext context) => _context = context;

    public async Task<Result<IEnumerable<PunchDto>>> Handle(GetPunchesInProjectQuery request, CancellationToken cancellationToken)
    {
        var punches =
            await (from punch in _context.QuerySet<Punch>()
                   join pro in _context.QuerySet<Project>()
                       on punch.ProjectId equals pro.Id
                   where pro.Name == request.ProjectName && (!punch.IsVoided || request.IncludeVoided)
                   select new PunchDto(
                       punch.Guid,
                       pro.Name,
                       punch.Title,
                       punch.IsVoided,
                       punch.RowVersion.ConvertToString())
                )
                .TagWith($"{nameof(GetPunchesInProjectQueryHandler)}.{nameof(Handle)}")
                .ToListAsync(cancellationToken);

        return new SuccessResult<IEnumerable<PunchDto>>(punches);
    }
}
