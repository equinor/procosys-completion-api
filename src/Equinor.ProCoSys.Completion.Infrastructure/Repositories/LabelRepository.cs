using System;
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

    public Task<List<Label>> GetManyAsync(IEnumerable<string> texts, CancellationToken cancellationToken)
    {
        var textsLowerCase = texts.Select(l => l.ToLower()).ToList();
        return DefaultQuery.Where(l => textsLowerCase.Contains(l.Text.ToLower()))
            .ToListAsync(cancellationToken);
    }

    public async Task<Label> GetByTextAsync(string text, CancellationToken cancellationToken)
    {
        var textLowerCase = text.ToLower();
        var label = await Set.Include(l => l.AvailableFor)
            .Where(l => l.Text.ToLower() == textLowerCase).SingleOrDefaultAsync(cancellationToken);
        if (label is null)
        {
            throw new Exception($"Label {text} not found");
        }
        return label;
    }
}
