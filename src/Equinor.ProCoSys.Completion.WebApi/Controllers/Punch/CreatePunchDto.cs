using System;
using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Punch;

public record CreatePunchDto(
    [Required]
    string ItemNo, 
    [Required]
    Guid ProjectGuid);
//{
//    [Required]
//    public string ItemNo { get; init; } = null!;

//    [Required]
//    public Guid ProjectGuid { get; init; }
//}
