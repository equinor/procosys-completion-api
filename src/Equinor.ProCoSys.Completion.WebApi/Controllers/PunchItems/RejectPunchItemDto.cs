using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;

public record RejectPunchItemDto(
    [Required]
    string Comment,
    [Required]
    string RowVersion);
