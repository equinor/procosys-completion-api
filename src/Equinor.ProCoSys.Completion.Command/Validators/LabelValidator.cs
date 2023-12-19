using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Command.Validators;

public class LabelValidator : ILabelValidator
{
    private readonly IReadOnlyContext _context;

    public LabelValidator(IReadOnlyContext context) => _context = context;

    public async Task<bool> ExistsAsync(string labelText, CancellationToken cancellationToken)
    {
        var label = await GetLabelAsync(labelText, cancellationToken);

        return label is not null;
    }

    public async Task<bool> IsVoidedAsync(string labelText, CancellationToken cancellationToken)
    {
        var label = await GetLabelAsync(labelText, cancellationToken);

        return label is not null && label.IsVoided;
    }

    private async Task<Label?> GetLabelAsync(string labelText, CancellationToken cancellationToken)
        => await (from label in _context.QuerySet<Label>()
            where label.Text == labelText
            select label).SingleOrDefaultAsync(cancellationToken);
}
