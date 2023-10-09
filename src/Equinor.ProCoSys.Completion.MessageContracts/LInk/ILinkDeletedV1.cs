using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.Link;

public interface ILinkDeletedV1 : IIntegrationEvent
{
    Guid SourceGuid { get; }
    Guid DeletedByOid { get; }
    DateTime DeletedAtUtc { get; }
}
