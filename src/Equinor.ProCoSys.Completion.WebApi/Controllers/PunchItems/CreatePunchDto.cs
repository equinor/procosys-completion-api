using System;
using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;

public record CreatePunchDto(
    [Required]
    string ItemNo, 
    [Required]
    Guid ProjectGuid);
