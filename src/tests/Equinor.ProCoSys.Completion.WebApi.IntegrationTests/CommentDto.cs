using System;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public record CommentDto(
    Guid ParentGuid,
    Guid Guid,
    string Text,
    PersonDto CreatedBy,
    DateTime CreatedAtUtc);
