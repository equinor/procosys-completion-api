using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.LabelEntityQueries.GetLabelsForEntity;

public class GetLabelsForEntityQueryHandler : IRequestHandler<GetLabelsForEntityQuery, Result<IEnumerable<string>>>
{
    private readonly IReadOnlyContext _context;

    public GetLabelsForEntityQueryHandler(IReadOnlyContext context) => _context = context;

    public async Task<Result<IEnumerable<string>>> Handle(GetLabelsForEntityQuery request, CancellationToken cancellationToken)
    {
        var labelEntityWithNonVoidedLabels =
            await (from lh in _context.QuerySet<LabelEntity>()
                        .Include(lh => lh.Labels.Where(l => !l.IsVoided))
                    where lh.EntityWithLabel == request.EntityWithLabelsType
                   select lh)
                .TagWith($"{nameof(GetLabelsForEntityQueryHandler)}.{nameof(Handle)}")
                .SingleOrDefaultAsync(cancellationToken);

        if (labelEntityWithNonVoidedLabels is null)
        {
            return new SuccessResult<IEnumerable<string>>(new List<string>());
        }

        var orderedNonVoidedLabels = labelEntityWithNonVoidedLabels.Labels.Select(l => l.Text).Order();

        return new SuccessResult<IEnumerable<string>>(orderedNonVoidedLabels);
    }
}
