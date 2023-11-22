using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.Attachment;

public interface IAttachmentCreatedV1 : IAttachment, IIntegrationEvent
{
    IUser CreatedBy { get; }
    DateTime CreatedAtUtc { get;  }
}
