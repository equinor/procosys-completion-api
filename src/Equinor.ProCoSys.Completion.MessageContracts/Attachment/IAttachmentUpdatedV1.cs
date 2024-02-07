using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.MessageContracts.Attachment;

public interface IAttachmentUpdatedV1 : IAttachment, IIntegrationEvent
{
    string Description { get; }
    int RevisionNumber { get; }
    List<string> Labels { get; }
    User ModifiedBy { get; }
    DateTime ModifiedAtUtc { get; }
}
