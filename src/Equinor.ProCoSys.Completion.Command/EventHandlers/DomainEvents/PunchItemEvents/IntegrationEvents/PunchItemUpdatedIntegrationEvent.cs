using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.PunchItem;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;

public record PunchItemUpdatedIntegrationEvent
(
    string DisplayName,
    Guid Guid,
    Guid ProjectGuid,
    string ProjectName,
    string ProjectDescription,
    int ItemNo,
    Guid ModifiedByOid,
    DateTime ModifiedAtUtc,
    Guid? ClearedByOid,
    DateTime? ClearedAtUtc,
    Guid? RejectedByOid,
    DateTime? RejectedAtUtc,
    Guid? VerifiedByOid,
    DateTime? VerifiedAtUtc,
    List<IProperty> Changes
) : IPunchItemUpdatedV1
{
    internal PunchItemUpdatedIntegrationEvent(PunchItemUpdatedDomainEvent punchItemDomainEvent) : this(
        "Punch item updated",
        punchItemDomainEvent.PunchItem,
        punchItemDomainEvent.Changes)
    {
    }

    internal PunchItemUpdatedIntegrationEvent(PunchItemClearedDomainEvent punchItemDomainEvent) : this(
        "Punch item cleared",
        punchItemDomainEvent.PunchItem,
        new List<IProperty>())
    {
    }

    internal PunchItemUpdatedIntegrationEvent(PunchItemUnclearedDomainEvent punchItemDomainEvent) : this(
        "Punch item uncleared",
        punchItemDomainEvent.PunchItem,
        new List<IProperty>())
    {
    }

    internal PunchItemUpdatedIntegrationEvent(PunchItemRejectedDomainEvent punchItemDomainEvent) : this(
        "Punch item rejected",
        punchItemDomainEvent.PunchItem,
        new List<IProperty>())
    {
    }

    internal PunchItemUpdatedIntegrationEvent(PunchItemVerifiedDomainEvent punchItemDomainEvent) : this(
        "Punch item verified",
        punchItemDomainEvent.PunchItem,
        new List<IProperty>())
    {
    }

    internal PunchItemUpdatedIntegrationEvent(PunchItemUnverifiedDomainEvent punchItemDomainEvent) : this(
        "Punch item unverified",
        punchItemDomainEvent.PunchItem,
        new List<IProperty>())
    {
    }

    internal PunchItemUpdatedIntegrationEvent(PunchItemCategoryUpdatedDomainEvent punchItemDomainEvent) : this(
        $"Punch item category changed to {punchItemDomainEvent.PunchItem.Category}",
        punchItemDomainEvent.PunchItem,
        new List<IProperty>
        {
            punchItemDomainEvent.Change
        })
    {
    }

    private PunchItemUpdatedIntegrationEvent(string displayName, PunchItem punchItem, List<IProperty> changes) : this(
        displayName,
        punchItem.Guid,
        punchItem.Project.Guid,
        punchItem.Project.Name,
        punchItem.Project.Description,
        punchItem.ItemNo,
        punchItem.ModifiedBy!.Guid,
        punchItem.ModifiedAtUtc!.Value,
        punchItem.ClearedBy?.Guid,
        punchItem.ClearedAtUtc,
        punchItem.RejectedBy?.Guid,
        punchItem.RejectedAtUtc,
        punchItem.VerifiedBy?.Guid,
        punchItem.VerifiedAtUtc,
        changes)
    {
    }
}
