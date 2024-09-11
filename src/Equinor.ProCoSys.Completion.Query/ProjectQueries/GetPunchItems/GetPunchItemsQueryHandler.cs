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

namespace Equinor.ProCoSys.Completion.Query.ProjectQueries.GetPunchItems;

public class GetPunchItemsQueryHandler : IRequestHandler<GetPunchItemsQuery, IEnumerable<PunchItemDto>>
{
    private readonly IReadOnlyContext _context;

    public GetPunchItemsQueryHandler(IReadOnlyContext context) => _context = context;

    public async Task<IEnumerable<PunchItemDto>> Handle(GetPunchItemsQuery request, CancellationToken cancellationToken)
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
                       punchItem.Category,
                       punchItem.Description,
                       punchItem.RowVersion.ConvertToString())
                )
                .TagWith($"{nameof(GetPunchItemsQueryHandler)}.{nameof(Handle)}")
                .ToListAsync(cancellationToken);

        return punchItems;
    }
}
