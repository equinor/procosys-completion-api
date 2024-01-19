using System;

namespace Equinor.ProCoSys.Completion.MessageContracts;

public interface IIntegrationEvent
{
    // The entity Guid the event was published for
    Guid Guid { get; }
}
