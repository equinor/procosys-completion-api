using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.Attachment;

public interface IAttachmentUpdatedV1 : IAttachment, IIntegrationEvent
{
    IUser ModifiedBy { get; }
    DateTime ModifiedAtUtc { get; }
}
