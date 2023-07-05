using System;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public record CommentDto(
    Guid SourceGuid,
    Guid Guid,
    string Text,
    PersonDto CreatedBy,
    DateTime CreatedAtUtc);
