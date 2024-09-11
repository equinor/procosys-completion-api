using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.Completion.Query.LibraryItemQueries.GetLibraryItems;

public class GetPunchLibraryItemsQueryHandler(IReadOnlyContext context)
    : IRequestHandler<GetPunchLibraryItemsQuery, IEnumerable<LibraryItemDto>>
{
    public async Task<IEnumerable<LibraryItemDto>> Handle(GetPunchLibraryItemsQuery request, CancellationToken cancellationToken)
    {
        var libraryItems =
            await (from libraryItem in context.QuerySet<LibraryItem>()
                        .Include(l => l.Classifications)
                   where libraryItem.IsVoided == false &&
                         request.LibraryTypes.Contains(libraryItem.Type) &&
                         (libraryItem.Type != LibraryType.COMM_PRIORITY ||
                          (libraryItem.Type == LibraryType.COMM_PRIORITY && 
                          libraryItem.Classifications.Any( c => c.Name == Classification.PunchPriority)))
                   select new LibraryItemDto(
                       libraryItem.Guid,
                       libraryItem.Code,
                       libraryItem.Description,
                       libraryItem.Type.ToString())
                )
                .TagWith($"{nameof(GetPunchLibraryItemsQueryHandler)}.{nameof(Handle)}")
                .ToListAsync(cancellationToken);

        var orderedLibraryItems = libraryItems.OrderBy(l => l.Code);

        return orderedLibraryItems;
    }
}
