using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers;

public class CreateCommentDto
{
    [Required]
    public string Text { get; set; } = null!;
}
