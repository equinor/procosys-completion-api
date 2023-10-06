using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.Attachment;

public interface IAttachmentCreatedV1 : IAttachment, IIntegrationEvent
{
    Guid CreatedByOid { get; }
    DateTime CreatedAtUtc { get;  }
}
