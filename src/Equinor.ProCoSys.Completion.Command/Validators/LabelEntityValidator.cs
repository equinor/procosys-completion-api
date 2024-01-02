using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Command.Validators;

public class LabelEntityValidator : ILabelEntityValidator
{
    private readonly IReadOnlyContext _context;

    public LabelEntityValidator(IReadOnlyContext context) => _context = context;

    public async Task<bool> ExistsAsync(EntityTypeWithLabel entityType, CancellationToken cancellationToken)
    {
        var label = await GetLabelEntityAsync(entityType, cancellationToken);

        return label is not null;
    }

    private async Task<LabelEntity?> GetLabelEntityAsync(
        EntityTypeWithLabel entityType,
        CancellationToken cancellationToken)
        => await (from labelEntity in _context.QuerySet<LabelEntity>()
            where labelEntity.EntityType == entityType
            select labelEntity).SingleOrDefaultAsync(cancellationToken);
}
