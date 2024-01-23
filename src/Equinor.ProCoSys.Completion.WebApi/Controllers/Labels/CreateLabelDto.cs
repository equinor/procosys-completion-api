using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Labels;

public record CreateLabelDto(
    [Required]
    string Text);
