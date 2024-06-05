using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.LibraryItemQueries.GetLibraryItems;

public class GetLibraryItemsQueryHandler(IReadOnlyContext context)
    : IRequestHandler<GetLibraryItemsQuery, Result<IEnumerable<LibraryItemDto>>>
{
    public async Task<Result<IEnumerable<LibraryItemDto>>> Handle(GetLibraryItemsQuery request, CancellationToken cancellationToken)
    {
        var libraryItems =
            await (from libraryItem in context.QuerySet<LibraryItem>()
                   where libraryItem.IsVoided == false && request.LibraryTypes.Contains(libraryItem.Type)
                   select new LibraryItemDto(
                       libraryItem.Guid,
                       libraryItem.Code,
                       libraryItem.Description,
                       libraryItem.Type.ToString())
                )
                .TagWith($"{nameof(GetLibraryItemsQueryHandler)}.{nameof(Handle)}")
                .ToListAsync(cancellationToken);

        var orderedLibraryItems = libraryItems.OrderBy(l => l.Code);

        return new SuccessResult<IEnumerable<LibraryItemDto>>(orderedLibraryItems);
    }
}
