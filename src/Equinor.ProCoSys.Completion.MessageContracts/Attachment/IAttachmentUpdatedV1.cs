using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.Attachment;

public interface IAttachmentUpdatedV1 : IAttachment, IIntegrationEvent
{
    Guid ModifiedByOid { get; }
    DateTime ModifiedAtUtc { get; }
}
