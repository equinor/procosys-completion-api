using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;

public record UpdatePunchItemDto(
    [Required]
    string Description,
    [Required]
    string RowVersion);
