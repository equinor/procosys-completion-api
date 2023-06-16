using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers;

public class RowVersionDto
{
    [Required]
    public string RowVersion { get; set; } = null!;
}
