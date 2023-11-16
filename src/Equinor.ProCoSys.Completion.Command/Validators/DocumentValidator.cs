using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Command.Validators;

public class DocumentValidator : IDocumentValidator
{
    private readonly IReadOnlyContext _context;

    // Trick to write LINQ queries to let EF create effective SQL queries is
    // 1) use Any
    // 2) select a projection with as few columns as needed
    public DocumentValidator(IReadOnlyContext context) => _context = context;

    public async Task<bool> ExistsAsync(Guid documentGuid, CancellationToken cancellationToken) =>
        await (from d in _context.QuerySet<Document>()
            where d.Guid == documentGuid
            select 1).AnyAsync(cancellationToken);

    public async Task<bool> IsVoidedAsync(Guid documentGuid, CancellationToken cancellationToken)
        => await (from d in _context.QuerySet<Document>()
            where d.Guid == documentGuid && d.IsVoided
            select 1).AnyAsync(cancellationToken);
}
