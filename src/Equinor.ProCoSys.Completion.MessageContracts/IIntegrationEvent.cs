using System;

namespace Equinor.ProCoSys.Completion.MessageContracts;

public interface IIntegrationEvent
{
    // A human readable name of the published event
    string DisplayName { get; }
    // The entity Guid the event was published for
    Guid Guid { get; }
}
