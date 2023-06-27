using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers;

public record CreateLinkDto(
    [Required]
    string Title,
    [Required]
    string Url);
