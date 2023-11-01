using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.Attachment;

public interface IAttachmentDeletedV1 : IIntegrationEvent
{
    Guid SourceGuid { get; }
    IUser DeletedBy { get; }
    DateTime DeletedAtUtc { get; }
}
