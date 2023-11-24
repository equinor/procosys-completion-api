using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.Link;

public interface ILinkDeletedV1 : IIntegrationEvent
{
    // Guid of the entity owning the Link
    Guid ParentGuid { get; }
    User DeletedBy { get; }
    DateTime DeletedAtUtc { get; }
}
