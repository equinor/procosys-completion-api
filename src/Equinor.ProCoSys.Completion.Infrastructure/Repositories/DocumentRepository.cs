using Equinor.ProCoSys.Completion.Domain.AggregateModels.DocumentAggregate;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class DocumentRepository : EntityWithGuidRepository<Document>, IDocumentRepository
{
    public DocumentRepository(CompletionContext context)
        : base(context, context.Documents)
            
    {
    }
}
