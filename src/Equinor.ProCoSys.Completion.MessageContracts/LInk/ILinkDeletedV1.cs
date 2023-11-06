using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.Link;

public interface ILinkDeletedV1 : IIntegrationEvent
{
    Guid SourceGuid { get; }
    IUser DeletedBy { get; }
    DateTime DeletedAtUtc { get; }
}
