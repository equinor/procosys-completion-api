using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Comments;

public record CreateCommentDto(
    [Required]
    string Text,
    [Required]
    List<string> Labels);
