using System;
using System.ComponentModel.DataAnnotations;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;

public record CreatePunchItemDto(
    [Required] Category Category,
    [Required] string Description,
    [Required] Guid CheckListGuid,
    [Required] Guid RaisedByOrgGuid,
    [Required] Guid ClearingByOrgGuid,
    Guid? ActionByPersonOid,
    DateTime? DueTimeUtc,
    Guid? PriorityGuid,
    Guid? SortingGuid,
    Guid? TypeGuid,
    int? Estimate,
    Guid? OriginalWorkOrderGuid,
    Guid? WorkOrderGuid,
    Guid? SWCRGuid,
    Guid? DocumentGuid,
    string? ExternalItemNo,
    bool MaterialRequired,
    DateTime? MaterialETAUtc,
    string? MaterialExternalNo);
