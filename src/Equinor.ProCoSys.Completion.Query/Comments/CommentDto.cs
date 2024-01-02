using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.Query.Comments;

public record CommentDto(
    Guid ParentGuid,
    Guid Guid,
    string Text,
    List<string> Labels,
    PersonDto CreatedBy,
    DateTime CreatedAtUtc);
// No need for expose RowVersion since we don't support Update or Delete of Comments 
