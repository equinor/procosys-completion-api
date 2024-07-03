using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.MessageContracts.Comment;

public interface ICommentCreatedEventV1 : IIntegrationEvent
{
    public string Plant { get; }
    public Guid ParentGuid { get; }
    public User CreatedBy { get; }
    public DateTime CreatedAtUtc { get;  }
    public string Text { get; }
    public IEnumerable<string> Labels { get;  }
}
