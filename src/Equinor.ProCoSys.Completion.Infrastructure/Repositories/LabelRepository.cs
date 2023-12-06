using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class LabelRepository : EntityRepository<Label>, ILabelRepository
{
    public LabelRepository(CompletionContext context)
        : base(context, context.Labels)
    {
    }

    public Task<List<Label>> GetManyAsync(IEnumerable<string> labels, CancellationToken cancellationToken)
    {
        var labelsLwr = labels.Select(l => l.ToLower()).ToList();
        return DefaultQuery.Where(l => labelsLwr.Contains(l.Text.ToLower()))
            .ToListAsync(cancellationToken);
    }
}
