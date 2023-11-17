using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Command.Validators;

public class LibraryItemValidator : ILibraryItemValidator
{
    private readonly IReadOnlyContext _context;

    public LibraryItemValidator(IReadOnlyContext context) => _context = context;

    public async Task<bool> ExistsAsync(Guid libraryItemGuid, CancellationToken cancellationToken)
    {
        var libraryItem = await GetLibraryItemAsync(libraryItemGuid, cancellationToken);

        return libraryItem is not null;
    }

    public async Task<bool> HasTypeAsync(
        Guid libraryItemGuid,
        LibraryType type,
        CancellationToken cancellationToken)
    {
        var libraryItem = await GetLibraryItemAsync(libraryItemGuid, cancellationToken);

        return libraryItem is not null && libraryItem.Type == type;
    }

    public async Task<bool> IsVoidedAsync(Guid libraryItemGuid, CancellationToken cancellationToken)
    {
        var libraryItem = await GetLibraryItemAsync(libraryItemGuid, cancellationToken);

        return libraryItem is not null && libraryItem.IsVoided;
    }

    private async Task<LibraryItem?> GetLibraryItemAsync(Guid libraryItemGuid, CancellationToken cancellationToken)
        => await (from li in _context.QuerySet<LibraryItem>()
            where li.Guid == libraryItemGuid
            select li).SingleOrDefaultAsync(cancellationToken);
}
