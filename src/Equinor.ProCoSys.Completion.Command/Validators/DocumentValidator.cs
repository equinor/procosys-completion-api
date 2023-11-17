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

    public DocumentValidator(IReadOnlyContext context) => _context = context;

    public async Task<bool> ExistsAsync(Guid documentGuid, CancellationToken cancellationToken)
    {
        var document = await GetDocumentAsync(documentGuid, cancellationToken);

        return document is not null;
    }

    public async Task<bool> IsVoidedAsync(Guid documentGuid, CancellationToken cancellationToken)
    {
        var document = await GetDocumentAsync(documentGuid, cancellationToken);

        return document is not null && document.IsVoided;
    }

    private async Task<Document?> GetDocumentAsync(Guid documentGuid, CancellationToken cancellationToken)
        => await (from d in _context.QuerySet<Document>()
            where d.Guid == documentGuid
            select d).SingleOrDefaultAsync(cancellationToken);
}
