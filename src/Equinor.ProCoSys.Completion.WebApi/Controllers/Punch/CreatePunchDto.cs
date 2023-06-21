using System;
using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Punch;

public class CreatePunchDto
{
    [Required]
    public string ItemNo { get; set; } = null!;

    [Required]
    public Guid ProjectGuid { get; set; }
}
