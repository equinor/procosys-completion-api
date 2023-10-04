using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.Link;

public interface ILinkCreatedV1 : ILink, IIntegrationEvent
{
    Guid CreatedByOid { get; }
    DateTime CreatedAtUtc { get;  }
}
