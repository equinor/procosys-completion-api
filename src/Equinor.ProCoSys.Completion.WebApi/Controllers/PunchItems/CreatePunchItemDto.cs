using System;
using System.ComponentModel.DataAnnotations;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;

public record CreatePunchItemDto(
    [Required]
    Category Category,
    [Required]
    string Description, 
    [Required]
    Guid ProjectGuid,
    [Required]
    Guid CheckListGuid,
    [Required]
    Guid RaisedByOrgGuid,
    [Required]
    Guid ClearingByOrgGuid,
    Guid? PriorityGuid = null,
    Guid? SortingGuid = null,
    Guid? TypeGuid = null);
