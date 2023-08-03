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

    // todo Has value conversions been considered or do we explicitly want LibraryItem.Type to be a string for ease of use other places in the code?
    public Task<LibraryItem?> GetByGuidAndTypeAsync(Guid libraryGuid, LibraryType type)
        => DefaultQuery.SingleOrDefaultAsync(x => x.Guid == libraryGuid && x.Type == type.ToString());

}
