using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class LibraryItemRepository : EntityWithGuidRepository<LibraryItem>, ILibraryItemRepository
{
    public LibraryItemRepository(CompletionContext context)
        : base(context, context.Library)
    {
    }
}
