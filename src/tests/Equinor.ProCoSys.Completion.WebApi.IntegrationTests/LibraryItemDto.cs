using System;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public record LibraryItemDto(
    Guid Guid,
    string Code,
    string Description);
