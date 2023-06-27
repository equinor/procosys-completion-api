using System;

namespace Equinor.ProCoSys.Completion.Query.Comments;

public record CommentDto(Guid SourceGuid,
    Guid Guid,
    string Text,
    PersonDto CreatedBy,
    DateTime CreatedAtUtc);
// No need for expose RowVersion since we don't support Update or Delete of Comments 
