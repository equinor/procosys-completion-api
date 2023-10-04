using System;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts.PunchItem;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;

public record PunchItemCreatedIntegrationEvent
(
    string DisplayName,
    Guid Guid,
    Guid ProjectGuid,
    string ProjectName,
    string ProjectDescription,
    int ItemNo,
    Guid CreatedByOid,
    DateTime CreatedAtUtc,
    Guid? ClearedByOid,
    DateTime? ClearedAtUtc,
    Guid? RejectedByOid,
    DateTime? RejectedAtUtc,
    Guid? VerifiedByOid,
    DateTime? VerifiedAtUtc
) : IPunchItemCreatedV1
{
    internal PunchItemCreatedIntegrationEvent(PunchItemCreatedDomainEvent punchItemCreatedEvent) : this(
        DisplayName: "Punch item created",
        punchItemCreatedEvent.PunchItem.Guid,
        punchItemCreatedEvent.PunchItem.Project.Guid,
        punchItemCreatedEvent.PunchItem.Project.Name,
        punchItemCreatedEvent.PunchItem.Project.Description,
        punchItemCreatedEvent.PunchItem.ItemNo,
        punchItemCreatedEvent.PunchItem.CreatedBy.Guid,
        punchItemCreatedEvent.PunchItem.CreatedAtUtc,
        punchItemCreatedEvent.PunchItem.ClearedBy?.Guid,
        punchItemCreatedEvent.PunchItem.ClearedAtUtc,
        punchItemCreatedEvent.PunchItem.RejectedBy?.Guid,
        punchItemCreatedEvent.PunchItem.RejectedAtUtc,
        punchItemCreatedEvent.PunchItem.VerifiedBy?.Guid,
        punchItemCreatedEvent.PunchItem.VerifiedAtUtc)
    { }
}
