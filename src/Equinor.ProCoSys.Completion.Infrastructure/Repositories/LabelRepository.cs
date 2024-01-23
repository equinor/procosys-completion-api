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
        var label = await GetLabelAsync(text, cancellationToken);
        if (label is null)
        {
            throw new Exception($"Label {text} not found");
        }
        return label;
    }

    public async Task<bool> ExistsAsync(string text, CancellationToken cancellationToken)
        => await GetLabelAsync(text, cancellationToken) is not null;

    private async Task<Label?> GetLabelAsync(string text, CancellationToken cancellationToken)
    {
        var textLowerCase = text.ToLower();
        var label = await Set.Include(l => l.AvailableFor)
            .Where(l => l.Text.ToLower() == textLowerCase).SingleOrDefaultAsync(cancellationToken);
        return label;
    }
}
