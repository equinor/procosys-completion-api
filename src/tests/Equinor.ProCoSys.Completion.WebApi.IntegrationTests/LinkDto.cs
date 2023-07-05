using System;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public record LinkDto(
    Guid SourceGuid,
    Guid Guid,
    string Title,
    string Url,
    string RowVersion);
