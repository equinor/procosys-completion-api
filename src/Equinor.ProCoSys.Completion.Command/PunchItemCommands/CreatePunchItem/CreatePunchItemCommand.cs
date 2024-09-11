using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;

public class CreatePunchItemCommand(
    Category category,
    string description,
    Guid checkListGuid,
    Guid raisedByOrgGuid,
    Guid clearingByOrgGuid,
    Guid? actionByPersonOid,
    DateTime? dueTimeUtc,
    Guid? priorityGuid,
    Guid? sortingGuid,
    Guid? typeGuid,
    int? estimate,
    Guid? originalWorkOrderGuid,
    Guid? workOrderGuid,
    Guid? swcrGuid,
    Guid? documentGuid,
    string? externalItemNo,
    bool materialRequired,
    DateTime? materialETAUtc,
    string? materialExternalNo)
    : ICanHaveRestrictionsViaCheckList, IRequest<GuidAndRowVersion>
{
    public Category Category { get; } = category;
    public string Description { get; } = description;
    public Guid CheckListGuid { get; } = checkListGuid;
    public Guid RaisedByOrgGuid { get; } = raisedByOrgGuid;
    public Guid ClearingByOrgGuid { get; } = clearingByOrgGuid;
    public Guid? ActionByPersonOid { get; } = actionByPersonOid;
    public DateTime? DueTimeUtc { get; } = dueTimeUtc;
    public Guid? PriorityGuid { get; } = priorityGuid;
    public Guid? SortingGuid { get; } = sortingGuid;
    public Guid? TypeGuid { get; } = typeGuid;
    public int? Estimate { get; } = estimate;
    public Guid? OriginalWorkOrderGuid { get; } = originalWorkOrderGuid;
    public Guid? WorkOrderGuid { get; } = workOrderGuid;
    public Guid? SWCRGuid { get; } = swcrGuid;
    public Guid? DocumentGuid { get; } = documentGuid;
    public string? ExternalItemNo { get; } = externalItemNo;
    public bool MaterialRequired { get; } = materialRequired;
    public DateTime? MaterialETAUtc { get; } = materialETAUtc;
    public string? MaterialExternalNo { get; } = materialExternalNo;
    public Guid GetProjectGuidForAccessCheck() => CheckListDetailsDto.ProjectGuid;
    public Guid GetCheckListGuidForWriteAccessCheck() => CheckListGuid;
    public CheckListDetailsDto CheckListDetailsDto { get; set; } = null!;
}
