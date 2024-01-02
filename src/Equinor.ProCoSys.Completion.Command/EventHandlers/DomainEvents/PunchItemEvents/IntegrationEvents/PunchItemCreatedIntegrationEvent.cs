using System;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.PunchItem;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;

public record PunchItemCreatedIntegrationEvent
(
    string DisplayName,
    Guid Guid,
    Guid ParentGuid,
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
    string Plant  //todo: skal ikke denne være med her? 
) : IPunchItemCreatedV1
{
    internal PunchItemCreatedIntegrationEvent(PunchItemCreatedDomainEvent domainEvent) : this(
        "Punch item created",
        domainEvent.PunchItem.Guid,
        domainEvent.PunchItem.CheckListGuid,
        domainEvent.PunchItem.Project.Guid,
        domainEvent.PunchItem.Project.Name,
        domainEvent.PunchItem.Project.Description,
        domainEvent.PunchItem.CheckListGuid,
        domainEvent.PunchItem.Category.ToString(),
        domainEvent.PunchItem.ItemNo,
        domainEvent.PunchItem.Description,
        domainEvent.PunchItem.RaisedByOrg.Code,
        domainEvent.PunchItem.RaisedByOrg.Guid,
        domainEvent.PunchItem.ClearingByOrg.Code,
        domainEvent.PunchItem.ClearingByOrg.Guid,
        domainEvent.PunchItem.Sorting?.Code,
        domainEvent.PunchItem.Sorting?.Guid,
        domainEvent.PunchItem.Type?.Code,
        domainEvent.PunchItem.Type?.Guid,
        domainEvent.PunchItem.Priority?.Code,
        domainEvent.PunchItem.Priority?.Guid,
        domainEvent.PunchItem.DueTimeUtc,
        domainEvent.PunchItem.Estimate,
        domainEvent.PunchItem.ExternalItemNo,
        domainEvent.PunchItem.MaterialRequired,
        domainEvent.PunchItem.MaterialETAUtc,
        domainEvent.PunchItem.MaterialExternalNo,
        domainEvent.PunchItem.WorkOrder?.No,
        domainEvent.PunchItem.WorkOrder?.Guid,
        domainEvent.PunchItem.OriginalWorkOrder?.No,
        domainEvent.PunchItem.OriginalWorkOrder?.Guid,
        domainEvent.PunchItem.Document?.No,
        domainEvent.PunchItem.Document?.Guid,
        domainEvent.PunchItem.SWCR?.No,
        domainEvent.PunchItem.SWCR?.Guid,
        domainEvent.PunchItem.ActionBy is null ?
            null :
            new User(domainEvent.PunchItem.ActionBy.Guid, domainEvent.PunchItem.ActionBy.GetFullName()),
        domainEvent.PunchItem.ClearedBy is null ?
            null :
            new User(domainEvent.PunchItem.ClearedBy.Guid, domainEvent.PunchItem.ClearedBy.GetFullName()),
        domainEvent.PunchItem.ClearedAtUtc,
        domainEvent.PunchItem.RejectedBy is null ?
            null :
            new User(domainEvent.PunchItem.RejectedBy.Guid, domainEvent.PunchItem.RejectedBy.GetFullName()),
        domainEvent.PunchItem.RejectedAtUtc,
        domainEvent.PunchItem.VerifiedBy is null ?
            null :
            new User(domainEvent.PunchItem.VerifiedBy.Guid, domainEvent.PunchItem.VerifiedBy.GetFullName()),
        domainEvent.PunchItem.VerifiedAtUtc,
        new User(domainEvent.PunchItem.CreatedBy.Guid, domainEvent.PunchItem.CreatedBy.GetFullName()),
        domainEvent.PunchItem.CreatedAtUtc,
        domainEvent.PunchItem.Plant)
    { }
}
