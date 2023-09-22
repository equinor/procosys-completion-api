using System;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;

public record PunchItemCreatedIntegrationEvent
(
    string DisplayName,
    Guid ProjectGuid,
    string ProjectName,
    string ProjectDescription,
    Guid Guid,
    int ItemNo,
    Guid CreatedByOid,
    DateTime CreatedAtUtc
) : IPunchItemCreatedV1
{
    internal PunchItemCreatedIntegrationEvent(PunchItemCreatedDomainEvent punchItemCreatedEvent) : this(
        DisplayName: "Punch item created",
        punchItemCreatedEvent.PunchItem.Project.Guid,
        punchItemCreatedEvent.PunchItem.Project.Name,
        punchItemCreatedEvent.PunchItem.Project.Description,
        punchItemCreatedEvent.PunchItem.Guid,
        punchItemCreatedEvent.PunchItem.ItemNo,
        punchItemCreatedEvent.PunchItem.CreatedByOid,
        punchItemCreatedEvent.PunchItem.CreatedAtUtc)
    { }
}
