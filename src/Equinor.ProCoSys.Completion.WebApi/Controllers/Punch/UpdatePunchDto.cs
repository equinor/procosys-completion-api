using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Punch;

public record UpdatePunchDto(
    [Required]
    string? Description,
    [Required]
    string RowVersion);
