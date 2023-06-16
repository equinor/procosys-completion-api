using System;

namespace Equinor.ProCoSys.Completion.Command.Comments;

public class CommentDto
{
    public CommentDto(Guid guid, string rowVersion)
    {
        Guid = guid;
        RowVersion = rowVersion;
    }

    public Guid Guid { get; }
    public string RowVersion { get; }
}
