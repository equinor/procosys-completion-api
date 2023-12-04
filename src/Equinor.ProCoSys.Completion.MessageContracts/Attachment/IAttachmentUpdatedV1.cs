using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.Attachment;

public interface IAttachmentUpdatedV1 : IAttachment, IIntegrationEvent
{
    User ModifiedBy { get; }
    DateTime ModifiedAtUtc { get; }
}
