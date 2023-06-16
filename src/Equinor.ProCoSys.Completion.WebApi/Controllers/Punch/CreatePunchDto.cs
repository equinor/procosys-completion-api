using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Punch;

public class CreatePunchDto
{
    [Required]
    public string Title { get; set; } = null!;

    [Required]
    public string ProjectName { get; set; } = null!;
}
