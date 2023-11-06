using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.Attachment;

public interface IAttachmentDeletedV1 : IIntegrationEvent
{
    // Guid of the entity owning the Attachment
    Guid ParentGuid { get; }
    IUser DeletedBy { get; }
    DateTime DeletedAtUtc { get; }
}
