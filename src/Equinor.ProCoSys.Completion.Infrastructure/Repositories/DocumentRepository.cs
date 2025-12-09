using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class DocumentRepository(CompletionContext context) : EntityWithGuidRepository<Document>(context, context.Documents), IDocumentRepository
{
    public Task<Document?> GetByNoAsync(string documentNo, CancellationToken cancellationToken) => DefaultQueryable
        .SingleOrDefaultAsync(x => x.No == documentNo, cancellationToken);
}
