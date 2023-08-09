using System;

namespace Equinor.ProCoSys.Completion.Query;

public record LibraryItemDto(
    Guid Guid,
    string Code,
    string Description);
