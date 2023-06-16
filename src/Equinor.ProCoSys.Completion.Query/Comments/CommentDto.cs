using System;

namespace Equinor.ProCoSys.Completion.Query.Comments;

public class CommentDto
{
    public CommentDto(
        Guid sourceGuid,
        Guid guid,
        string text,
        PersonDto createdBy,
        DateTime createdAtUtc)
    {
        SourceGuid = sourceGuid;
        Guid = guid;
        Text = text;
        CreatedBy = createdBy;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid SourceGuid { get; }
    public Guid Guid { get; }
    public string Text { get; }
    public PersonDto CreatedBy { get; }
    public DateTime CreatedAtUtc { get; }
    
    // No need for expose RowVersion since we don't support Update or Delete of Comments 
}
