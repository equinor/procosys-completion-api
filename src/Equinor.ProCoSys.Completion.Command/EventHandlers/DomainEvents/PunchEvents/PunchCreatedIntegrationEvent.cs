using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;
using Equinor.ProCoSys.Completion.MessageContracts;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchEvents;
public record PunchCreatedIntegrationEvent
(
    string DisplayName,
    Guid ProjectGuid ,
    Guid Guid ,
    string ItemNo ,
    Guid CreatedByOid,
    DateTime CreatedAtUtc
) : IPunchCreatedV1
{
    internal PunchCreatedIntegrationEvent(PunchCreatedEvent punchCreatedEvent) : this(
        DisplayName:"Punch created",
        punchCreatedEvent.ProjectGuid,
        punchCreatedEvent.Punch.Guid,
        punchCreatedEvent.Punch.ItemNo,
        punchCreatedEvent.Punch.CreatedByOid,
        punchCreatedEvent.Punch.CreatedAtUtc) { }
    
}
