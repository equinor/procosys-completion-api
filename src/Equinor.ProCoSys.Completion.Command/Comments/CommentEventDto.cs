using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.MessageContracts;

namespace Equinor.ProCoSys.Completion.Command.Comments;

public record CommentEventDto
{
    public required Guid Guid { get; set; }

    public required string Plant { get; set; }

    public required Guid ParentGuid { get; set; }

    public required User CreatedBy { get; set; }

    public required DateTime CreatedAtUtc { get; set; }

    public required string Text { get; set; }

    public required List<string> Labels { get; set; }
}
