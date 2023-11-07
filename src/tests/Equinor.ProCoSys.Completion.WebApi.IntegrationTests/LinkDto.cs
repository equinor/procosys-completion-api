using System;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public record LinkDto(
    Guid ParentGuid,
    Guid Guid,
    string Title,
    string Url,
    string RowVersion);
