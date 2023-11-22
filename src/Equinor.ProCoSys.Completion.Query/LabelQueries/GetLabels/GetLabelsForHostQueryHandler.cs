using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelHostAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.LabelQueries.GetLabels;

public class GetLabelsForHostQueryHandler : IRequestHandler<GetLabelsForHostQuery, Result<IEnumerable<string>>>
{
    private readonly IReadOnlyContext _context;

    public GetLabelsForHostQueryHandler(IReadOnlyContext context) => _context = context;

    public async Task<Result<IEnumerable<string>>> Handle(GetLabelsForHostQuery request, CancellationToken cancellationToken)
    {
        var labelHost =
            await (from lh in _context.QuerySet<LabelHost>()
                        .Include(lh => lh.Labels)
                    where lh.Type ==  request.HostType
                   select lh)
                .TagWith($"{nameof(GetLabelsForHostQueryHandler)}.{nameof(Handle)}")
                .SingleOrDefaultAsync(cancellationToken);

        if (labelHost is null)
        {
            return new SuccessResult<IEnumerable<string>>(new List<string>());
        }

        var orderedLabels = labelHost.Labels.Select(l => l.Text).Order();

        return new SuccessResult<IEnumerable<string>>(orderedLabels);
    }
}
