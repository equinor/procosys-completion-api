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

namespace Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchItemsInProject;

public class GetPunchItemsInProjectQueryHandler : IRequestHandler<GetPunchItemsInProjectQuery, Result<IEnumerable<PunchDto>>>
{
    private readonly IReadOnlyContext _context;

    public GetPunchItemsInProjectQueryHandler(IReadOnlyContext context) => _context = context;

    public async Task<Result<IEnumerable<PunchDto>>> Handle(GetPunchItemsInProjectQuery request, CancellationToken cancellationToken)
    {
        var punchItems =
            await (from punch in _context.QuerySet<Punch>()
                   join project in _context.QuerySet<Project>()
                       on punch.ProjectId equals project.Id
                   where project.Guid == request.ProjectGuid
                   select new PunchDto(
                       punch.Guid,
                       project.Name,
                       punch.ItemNo,
                       punch.RowVersion.ConvertToString())
                )
                .TagWith($"{nameof(GetPunchItemsInProjectQueryHandler)}.{nameof(Handle)}")
                .ToListAsync(cancellationToken);

        return new SuccessResult<IEnumerable<PunchDto>>(punchItems);
    }
}
