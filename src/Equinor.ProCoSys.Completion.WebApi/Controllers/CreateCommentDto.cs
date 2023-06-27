using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers;

public record CreateCommentDto(
    [Required]
    string Text);
