using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;

namespace Equinor.ProCoSys.Completion.Query;

public record LibraryItemDto(
    Guid Guid,
    string Code,
    string Description,
    string LibraryType);
