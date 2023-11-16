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

    // Trick to write LINQ queries to let EF create effective SQL queries is
    // 1) use Any
    // 2) select a projection with as few columns as needed
    public LibraryItemValidator(IReadOnlyContext context) => _context = context;

    public async Task<bool> ExistsAsync(Guid libraryItemGuid, CancellationToken cancellationToken) =>
        await (from l in _context.QuerySet<LibraryItem>()
            where l.Guid == libraryItemGuid
            select 1).AnyAsync(cancellationToken);

    public async Task<bool> HasTypeAsync(
        Guid libraryItemGuid,
        LibraryType type,
        CancellationToken cancellationToken)
        => await (from l in _context.QuerySet<LibraryItem>()
            where l.Guid == libraryItemGuid && l.Type == type
            select 1).AnyAsync(cancellationToken);

    public async Task<bool> IsVoidedAsync(Guid libraryItemGuid, CancellationToken cancellationToken)
        => await (from l in _context.QuerySet<LibraryItem>()
            where l.Guid == libraryItemGuid && l.IsVoided == true
            select 1).AnyAsync(cancellationToken);
}
