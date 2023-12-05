using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Microsoft.EntityFrameworkCore;
using System;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class LabelRepository : EntityRepository<Label>, ILabelRepository
{
    public LabelRepository(CompletionContext context)
        : base(context, context.Labels)
    {
    }

    public Task<List<Label>> GetManyAsync(IEnumerable<string> labels, CancellationToken cancellationToken) =>
        DefaultQuery.Where(l =>
                labels.Any(label => l.Text.Equals(label, StringComparison.OrdinalIgnoreCase)))
            .ToListAsync(cancellationToken);
}
