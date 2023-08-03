using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Command.Validators.LibraryItemValidators;

public class LibraryItemValidator : ILibraryItemValidator
{
    private readonly IReadOnlyContext _context;

    public LibraryItemValidator(IReadOnlyContext context) => _context = context;

    public async Task<bool> ExistsAsync(Guid libraryItemGuid, CancellationToken cancellationToken) =>
        await (from l in _context.QuerySet<LibraryItem>()
            where l.Guid == libraryItemGuid
            select l).AnyAsync(cancellationToken);

    public async Task<bool> HasTypeAsync(
        Guid libraryItemGuid,
        LibraryTypes type,
        CancellationToken cancellationToken)
    {
        var libraryItem = await(from p in _context.QuerySet<LibraryItem>()
            where p.Guid == libraryItemGuid
            select p).SingleOrDefaultAsync(cancellationToken);

        return libraryItem is not null && libraryItem.Type == type.ToString();
    }

    public async Task<bool> IsVoidedAsync(Guid libraryItemGuid, CancellationToken cancellationToken)
    {
        var libraryItem = await (from p in _context.QuerySet<LibraryItem>()
            where p.Guid == libraryItemGuid
            select p).SingleOrDefaultAsync(cancellationToken);

        return libraryItem is not null && libraryItem.IsVoided;
    }
}
