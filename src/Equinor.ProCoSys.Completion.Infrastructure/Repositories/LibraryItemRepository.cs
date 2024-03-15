using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class LibraryItemRepository : EntityWithGuidRepository<LibraryItem>, ILibraryItemRepository
{
    public LibraryItemRepository(CompletionContext context)
        : base(context, context.Library)
    {
    }

    public async Task<LibraryItem> GetByGuidAndTypeAsync(
        Guid libraryGuid,
        LibraryType type,
        CancellationToken cancellationToken)
        => await DefaultQuery
               .SingleOrDefaultAsync(x => x.Guid == libraryGuid && x.Type == type, cancellationToken)
           ?? throw new EntityNotFoundException<LibraryItem>(libraryGuid.ToString());
}
