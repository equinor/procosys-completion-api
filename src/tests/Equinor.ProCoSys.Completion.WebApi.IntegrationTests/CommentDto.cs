using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public record CommentDto(
    Guid ParentGuid,
    Guid Guid,
    string Text,
    List<string> Labels,
    PersonDto CreatedBy,
    DateTime CreatedAtUtc);
