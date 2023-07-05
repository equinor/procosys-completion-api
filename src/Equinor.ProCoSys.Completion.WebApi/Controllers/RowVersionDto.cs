using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers;

public record RowVersionDto(
    [Required]
    string RowVersion);
