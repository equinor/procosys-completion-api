using System;

namespace Equinor.ProCoSys.Completion.Query.LibraryItemQueries.GetLibraryItems;

public record LibraryItemDto(
    Guid Guid,
    string Code,
    string Description);
