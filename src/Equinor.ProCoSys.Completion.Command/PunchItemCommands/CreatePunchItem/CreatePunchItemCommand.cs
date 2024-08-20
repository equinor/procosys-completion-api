using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;

public class CreatePunchItemCommand
    : ICanHaveRestrictionsViaCheckList, IIsCheckListCommand, IRequest<Result<GuidAndRowVersion>>
{
    public CreatePunchItemCommand(
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
    {
        Category = category;
        Description = description;
        CheckListGuid = checkListGuid;
        RaisedByOrgGuid = raisedByOrgGuid;
        ClearingByOrgGuid = clearingByOrgGuid;
        ActionByPersonOid = actionByPersonOid;
        DueTimeUtc = dueTimeUtc;
        PriorityGuid = priorityGuid;
        SortingGuid = sortingGuid;
        TypeGuid = typeGuid;
        Estimate = estimate;
        OriginalWorkOrderGuid = originalWorkOrderGuid;
        WorkOrderGuid = workOrderGuid;
        SWCRGuid = swcrGuid;
        DocumentGuid = documentGuid;
        ExternalItemNo = externalItemNo;
        MaterialRequired = materialRequired;
        MaterialETAUtc = materialETAUtc;
        MaterialExternalNo = materialExternalNo;
    }

    public Category Category { get; }
    public string Description { get; }
    public Guid CheckListGuid { get; }
    public CheckListDetailsDto CheckListDetailsDto { get; set; } = null!;
    public Guid RaisedByOrgGuid { get; }
    public Guid ClearingByOrgGuid { get; }
    public Guid? ActionByPersonOid { get; }
    public DateTime? DueTimeUtc { get; }
    public Guid? PriorityGuid { get; }
    public Guid? SortingGuid { get; }
    public Guid? TypeGuid { get; }
    public int? Estimate { get; }
    public Guid? OriginalWorkOrderGuid { get; }
    public Guid? WorkOrderGuid { get; }
    public Guid? SWCRGuid { get; }
    public Guid? DocumentGuid { get; }
    public string? ExternalItemNo { get; }
    public bool MaterialRequired { get; }
    public DateTime? MaterialETAUtc { get; }
    public string? MaterialExternalNo { get; }
    public Guid GetProjectGuidForAccessCheck() => CheckListDetailsDto.ProjectGuid;
    public Guid GetCheckListGuidForWriteAccessCheck() => CheckListGuid;
}
