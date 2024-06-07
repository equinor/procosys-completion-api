using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Infrastructure.Repositories;

public class LibraryItemRepository(CompletionContext context)
    : EntityWithGuidRepository<LibraryItem>(context, context.Library),
        ILibraryItemRepository
{
    public async Task<LibraryItem> GetByGuidAndTypeAsync(
        Guid libraryGuid,
        LibraryType type,
        CancellationToken cancellationToken)
        => await DefaultQueryable
               .SingleOrDefaultAsync(x => x.Guid == libraryGuid && x.Type == type, cancellationToken)
           ?? throw new EntityNotFoundException<LibraryItem>(libraryGuid);


    public async Task<LibraryItem> GetUnknownOrgAsync(string busEventPlant, CancellationToken cancellationToken)
    {
        const string LibraryCodeForUnknownOrg = "UNKNOWN";
        var libraryItem = await Set.SingleAsync(l
            => l.Type == LibraryType.COMPLETION_ORGANIZATION && l.Code == LibraryCodeForUnknownOrg, cancellationToken);

        return libraryItem;
    }
}
