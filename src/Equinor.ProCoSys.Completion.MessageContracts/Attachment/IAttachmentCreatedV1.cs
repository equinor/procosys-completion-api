using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.Attachment;

public interface IAttachmentCreatedV1 : IAttachment, IIntegrationEvent
{
    User CreatedBy { get; }
    DateTime CreatedAtUtc { get;  }
}
