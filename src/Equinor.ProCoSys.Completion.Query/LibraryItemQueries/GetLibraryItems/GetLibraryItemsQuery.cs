﻿using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.LibraryItemQueries.GetLibraryItems;

public class GetLibraryItemsQuery : IRequest<Result<IEnumerable<LibraryItemDto>>>
{
    public GetLibraryItemsQuery(LibraryType[] libraryTypes) => LibraryTypes = libraryTypes;

    public LibraryType[] LibraryTypes { get; }
}
