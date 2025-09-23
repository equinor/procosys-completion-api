using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers;

public sealed record RowVersionDto(
    [Required]
    string RowVersion);
