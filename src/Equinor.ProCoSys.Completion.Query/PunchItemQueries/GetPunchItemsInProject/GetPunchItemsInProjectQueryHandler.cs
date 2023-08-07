using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsInProject;

public class GetPunchItemsInProjectQueryHandler : IRequestHandler<GetPunchItemsInProjectQuery, Result<IEnumerable<PunchItemDto>>>
{
    private readonly IReadOnlyContext _context;

    public GetPunchItemsInProjectQueryHandler(IReadOnlyContext context) => _context = context;

    public async Task<Result<IEnumerable<PunchItemDto>>> Handle(GetPunchItemsInProjectQuery request, CancellationToken cancellationToken)
    {
        var punchItems =
            await (from punchItem in _context.QuerySet<PunchItem>()
                   join project in _context.QuerySet<Project>()
                       on punchItem.ProjectId equals project.Id
                   where project.Guid == request.ProjectGuid
                   select new PunchItemDto(
                       punchItem.Guid,
                       project.Name,
                       punchItem.ItemNo,
                       punchItem.Description,
                       punchItem.RowVersion.ConvertToString())
                )
                .TagWith($"{nameof(GetPunchItemsInProjectQueryHandler)}.{nameof(Handle)}")
                .ToListAsync(cancellationToken);

        return new SuccessResult<IEnumerable<PunchItemDto>>(punchItems);
    }
}
