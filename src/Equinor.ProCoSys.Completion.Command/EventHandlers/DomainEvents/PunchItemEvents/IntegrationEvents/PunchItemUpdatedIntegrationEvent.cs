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
    Guid CheckListGuid,
    string Category,
    int ItemNo,
    string Description,
    string RaisedByOrgCode,
    Guid RaisedByOrgGuid,
    string ClearingByOrgCode,
    Guid ClearingByOrgGuid,
    string? SortingCode,
    Guid? SortingGuid,
    string? TypeCode,
    Guid? TypeGuid,
    string? PriorityCode,
    Guid? PriorityGuid,
    DateTime? DueTimeUtc,
    int? Estimate,
    string? ExternalItemNo,
    bool MaterialRequired,
    DateTime? MaterialETAUtc,
    string? MaterialExternalNo,
    string? WorkOrderNo,
    Guid? WorkOrderGuid,
    string? OriginalWorkOrderNo,
    Guid? OriginalWorkOrderGuid,
    string? DocumentNo,
    Guid? DocumentGuid,
    int? SWCRNo,
    Guid? SWCRGuid,
    User? ActionBy,
    User? ClearedBy,
    DateTime? ClearedAtUtc,
    User? RejectedBy,
    DateTime? RejectedAtUtc,
    User? VerifiedBy,
    DateTime? VerifiedAtUtc,
    User CreatedBy,
    DateTime CreatedAtUtc,
    User ModifiedBy,
    DateTime ModifiedAtUtc,
    List<IProperty> Changes
) : IPunchItemUpdatedV1
{
    internal PunchItemUpdatedIntegrationEvent(PunchItemUpdatedDomainEvent domainEvent) : this(
        "Punch item updated",
        domainEvent.PunchItem,
        domainEvent.Changes)
    {
    }

    internal PunchItemUpdatedIntegrationEvent(PunchItemClearedDomainEvent domainEvent) : this(
        "Punch item cleared",
        domainEvent.PunchItem,
        new List<IProperty>())
    {
    }

    internal PunchItemUpdatedIntegrationEvent(PunchItemUnclearedDomainEvent domainEvent) : this(
        "Punch item uncleared",
        domainEvent.PunchItem,
        new List<IProperty>())
    {
    }

    internal PunchItemUpdatedIntegrationEvent(PunchItemRejectedDomainEvent domainEvent) : this(
        "Punch item rejected",
        domainEvent.PunchItem,
        domainEvent.Changes)
    {
    }

    internal PunchItemUpdatedIntegrationEvent(PunchItemVerifiedDomainEvent domainEvent) : this(
        "Punch item verified",
        domainEvent.PunchItem,
        new List<IProperty>())
    {
    }

    internal PunchItemUpdatedIntegrationEvent(PunchItemUnverifiedDomainEvent domainEvent) : this(
        "Punch item unverified",
        domainEvent.PunchItem,
        new List<IProperty>())
    {
    }

    internal PunchItemUpdatedIntegrationEvent(PunchItemCategoryUpdatedDomainEvent domainEvent) : this(
        $"Punch item category changed to {domainEvent.PunchItem.Category}",
        domainEvent.PunchItem,
        new List<IProperty> { domainEvent.Change })
    {
    }

    private PunchItemUpdatedIntegrationEvent(string displayName, PunchItem punchItem, List<IProperty> changes) : this(
        displayName,
        punchItem.Guid,
        punchItem.Project.Guid,
        punchItem.Project.Name,
        punchItem.Project.Description,
        punchItem.CheckListGuid,
        punchItem.Category.ToString(),
        punchItem.ItemNo,
        punchItem.Description,
        punchItem.RaisedByOrg.Code,
        punchItem.RaisedByOrg.Guid,
        punchItem.ClearingByOrg.Code,
        punchItem.ClearingByOrg.Guid,
        punchItem.Sorting?.Code,
        punchItem.Sorting?.Guid,
        punchItem.Type?.Code,
        punchItem.Type?.Guid,
        punchItem.Priority?.Code,
        punchItem.Priority?.Guid,
        punchItem.DueTimeUtc,
        punchItem.Estimate,
        punchItem.ExternalItemNo,
        punchItem.MaterialRequired,
        punchItem.MaterialETAUtc,
        punchItem.MaterialExternalNo,
        punchItem.WorkOrder?.No,
        punchItem.WorkOrder?.Guid,
        punchItem.OriginalWorkOrder?.No,
        punchItem.OriginalWorkOrder?.Guid,
        punchItem.Document?.No,
        punchItem.Document?.Guid,
        punchItem.SWCR?.No,
        punchItem.SWCR?.Guid,
        punchItem.ActionBy is null ? null : new User(punchItem.ActionBy.Guid, punchItem.ActionBy.GetFullName()),
        punchItem.ClearedBy is null ? null : new User(punchItem.ClearedBy.Guid, punchItem.ClearedBy.GetFullName()),
        punchItem.ClearedAtUtc,
        punchItem.RejectedBy is null ? null : new User(punchItem.RejectedBy.Guid, punchItem.RejectedBy.GetFullName()),
        punchItem.RejectedAtUtc,
        punchItem.VerifiedBy is null ? null : new User(punchItem.VerifiedBy.Guid, punchItem.VerifiedBy.GetFullName()),
        punchItem.VerifiedAtUtc,
        new User(punchItem.CreatedBy.Guid, punchItem.CreatedBy.GetFullName()),
        punchItem.CreatedAtUtc,
        new User(punchItem.ModifiedBy!.Guid, punchItem.ModifiedBy!.GetFullName()),
        punchItem.ModifiedAtUtc!.Value,
        changes)
    {
    }
}
