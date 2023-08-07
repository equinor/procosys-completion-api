using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.LibraryItemQueries.GetLibraryItems;

public class GetLibraryItemsQuery : IRequest<Result<IEnumerable<LibraryItemDto>>>
{
    public GetLibraryItemsQuery(string type) => Type = type;

    public string Type { get; }
}
