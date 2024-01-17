using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.MessageContracts.History;

namespace Equinor.ProCoSys.Completion.MessageContracts.Attachment;

public interface IAttachmentUpdatedV1 : IAttachment, IIntegrationEvent
{
    string Description { get; }
    int RevisionNumber { get; }
    List<string> Labels { get; }
    User ModifiedBy { get; }
    DateTime ModifiedAtUtc { get; }
    List<IChangedProperty> Changes { get; }
}
