using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.Link;

public interface ILinkUpdatedV1 : ILink, IIntegrationEvent
{
    User ModifiedBy { get; }
    DateTime ModifiedAtUtc { get; }
}
