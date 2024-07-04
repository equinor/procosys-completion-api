using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.LibraryItemQueries.GetLibraryItems;

public class GetPunchLibraryItemsQuery : IRequest<Result<IEnumerable<LibraryItemDto>>>
{
    public GetPunchLibraryItemsQuery(LibraryType[] libraryTypes) => LibraryTypes = libraryTypes;

    public LibraryType[] LibraryTypes { get; }
}
