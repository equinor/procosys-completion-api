using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;

public record UpdatePunchDto(
    [Required]
    string? Description,
    [Required]
    string RowVersion);
