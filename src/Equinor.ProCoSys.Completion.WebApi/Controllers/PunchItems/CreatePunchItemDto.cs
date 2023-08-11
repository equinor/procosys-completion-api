using System;
using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;

public record CreatePunchItemDto(
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
