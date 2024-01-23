using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public record CommentDto(
    Guid ParentGuid,
    Guid Guid,
    string Text,
    List<string> Labels,
    List<PersonDto> Mentions,
    PersonDto CreatedBy,
    DateTime CreatedAtUtc);
