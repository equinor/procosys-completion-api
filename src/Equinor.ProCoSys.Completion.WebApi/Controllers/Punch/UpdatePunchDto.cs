using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Punch;

public class UpdatePunchDto
{
    [Required]
    public string Title { get; set; } = null!;
    public string? Text { get; set; }
    [Required]
    public string RowVersion { get; set; } = null!;
}
