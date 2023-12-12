using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.LabelEntityQueries.GetLabelsForEntityType;

public class GetLabelsForEntityTypeQueryHandler : IRequestHandler<GetLabelsForEntityTypeQuery, Result<IEnumerable<string>>>
{
    private readonly IReadOnlyContext _context;

    public GetLabelsForEntityTypeQueryHandler(IReadOnlyContext context) => _context = context;

    public async Task<Result<IEnumerable<string>>> Handle(GetLabelsForEntityTypeQuery request, CancellationToken cancellationToken)
    {
        var labelEntityWithNonVoidedLabels =
            await (from lh in _context.QuerySet<LabelEntity>()
                        .Include(lh => lh.Labels.Where(l => !l.IsVoided))
                    where lh.EntityType == request.EntityType
                   select lh)
                .TagWith($"{nameof(GetLabelsForEntityTypeQueryHandler)}.{nameof(Handle)}")
                .SingleOrDefaultAsync(cancellationToken);

        if (labelEntityWithNonVoidedLabels is null)
        {
            return new SuccessResult<IEnumerable<string>>(new List<string>());
        }

        var orderedNonVoidedLabels = labelEntityWithNonVoidedLabels.Labels.Select(l => l.Text).Order();

        return new SuccessResult<IEnumerable<string>>(orderedNonVoidedLabels);
    }
}
