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
    Guid CheckListGuid,
    string Category,
    int ItemNo,
    string Description,
    string RaisedByOrgCode,
    string ClearingByOrgCode,
    string? SortingCode,
    string? TypeCode,
    string? PriorityCode,
    DateTime? DueTimeUtc,
    int? Estimate,
    string? ExternalItemNo,
    bool MaterialRequired,
    DateTime? MaterialETAUtc,
    string? MaterialExternalNo,
    string? WorkOrderNo,
    string? OriginalWorkOrderNo,
    string? DocumentNo,
    int? SWCRNo,
    Guid? ActionByOid,
    Guid? ClearedByOid,
    DateTime? ClearedAtUtc,
    Guid? RejectedByOid,
    DateTime? RejectedAtUtc,
    Guid? VerifiedByOid,
    DateTime? VerifiedAtUtc,
    Guid CreatedByOid,
    DateTime CreatedAtUtc
) : IPunchItemCreatedV1
{
    internal PunchItemCreatedIntegrationEvent(PunchItemCreatedDomainEvent punchItemCreatedEvent) : this(
        "Punch item created",
        punchItemCreatedEvent.PunchItem.Guid,
        punchItemCreatedEvent.PunchItem.Project.Guid,
        punchItemCreatedEvent.PunchItem.Project.Name,
        punchItemCreatedEvent.PunchItem.Project.Description,
        punchItemCreatedEvent.PunchItem.CheckListGuid,
        punchItemCreatedEvent.PunchItem.Category.ToString(),
        punchItemCreatedEvent.PunchItem.ItemNo,
        punchItemCreatedEvent.PunchItem.Description,
        punchItemCreatedEvent.PunchItem.RaisedByOrg.Code,
        punchItemCreatedEvent.PunchItem.ClearingByOrg.Code,
        punchItemCreatedEvent.PunchItem.Sorting?.Code,
        punchItemCreatedEvent.PunchItem.Type?.Code,
        punchItemCreatedEvent.PunchItem.Priority?.Code,
        punchItemCreatedEvent.PunchItem.DueTimeUtc,
        punchItemCreatedEvent.PunchItem.Estimate,
        punchItemCreatedEvent.PunchItem.ExternalItemNo,
        punchItemCreatedEvent.PunchItem.MaterialRequired,
        punchItemCreatedEvent.PunchItem.MaterialETAUtc,
        punchItemCreatedEvent.PunchItem.MaterialExternalNo,
        punchItemCreatedEvent.PunchItem.WorkOrder?.No,
        punchItemCreatedEvent.PunchItem.OriginalWorkOrder?.No,
        punchItemCreatedEvent.PunchItem.Document?.No,
        punchItemCreatedEvent.PunchItem.SWCR?.No,
        punchItemCreatedEvent.PunchItem.ActionBy?.Guid,
        punchItemCreatedEvent.PunchItem.ClearedBy?.Guid,
        punchItemCreatedEvent.PunchItem.ClearedAtUtc,
        punchItemCreatedEvent.PunchItem.RejectedBy?.Guid,
        punchItemCreatedEvent.PunchItem.RejectedAtUtc,
        punchItemCreatedEvent.PunchItem.VerifiedBy?.Guid,
        punchItemCreatedEvent.PunchItem.VerifiedAtUtc,
        punchItemCreatedEvent.PunchItem.CreatedBy.Guid,
        punchItemCreatedEvent.PunchItem.CreatedAtUtc)
    { }
}
