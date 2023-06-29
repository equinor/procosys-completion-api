using System;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;
using Equinor.ProCoSys.Completion.MessageContracts;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchEvents;
public record PunchCreatedMessage
(
    string DisplayName,
    Guid ProjectGuid ,
    Guid Guid ,
    string ItemNo ,
    Guid CreatedByOid,
    DateTime CreatedAtUtc
) : IPunchCreatedV1
{
    internal PunchCreatedMessage(PunchCreatedEvent punchCreatedEvent) : this(
        "Punch created",
        punchCreatedEvent.ProjectGuid,
        punchCreatedEvent.Punch.Guid,
        punchCreatedEvent.Punch.ItemNo,
        punchCreatedEvent.Punch.CreatedByOid,
        punchCreatedEvent.Punch.CreatedAtUtc) { }
 
}
