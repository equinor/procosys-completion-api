using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class LibraryItemRepository : EntityWithGuidRepository<LibraryItem>, ILibraryItemRepository
{
    public LibraryItemRepository(CompletionContext context)
        : base(context, context.Library)
    {
    }

    public Task<LibraryItem?> GetByGuidAndTypeAsync(Guid libraryGuid, LibraryType type)
        => DefaultQuery.SingleOrDefaultAsync(x => x.Guid == libraryGuid && x.Type == type);

}
