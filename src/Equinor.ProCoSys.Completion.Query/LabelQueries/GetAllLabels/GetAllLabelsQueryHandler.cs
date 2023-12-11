using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.LabelQueries.GetAllLabels;

public class GetAllLabelsQueryHandler : IRequestHandler<GetAllLabelsQuery, Result<IEnumerable<LabelDto>>>
{
    private readonly IReadOnlyContext _context;

    public GetAllLabelsQueryHandler(IReadOnlyContext context) => _context = context;

    public async Task<Result<IEnumerable<LabelDto>>> Handle(GetAllLabelsQuery request, CancellationToken cancellationToken)
    {
        var orderedLabels =
            await (from l in _context.QuerySet<Label>()
                        .Include(l => l.EntitiesWithLabel)
                    orderby l.Text
                   select l)
                .TagWith($"{nameof(GetAllLabelsQueryHandler)}.{nameof(Handle)}")
                .ToListAsync(cancellationToken);

        var orderedLabelDtos = orderedLabels
            .Select(l => new LabelDto(l.Text, l.IsVoided, l.EntitiesWithLabel.Select(h => h.EntityWithLabel.ToString()).Order().ToList()));

        return new SuccessResult<IEnumerable<LabelDto>>(orderedLabelDtos);
    }
}
