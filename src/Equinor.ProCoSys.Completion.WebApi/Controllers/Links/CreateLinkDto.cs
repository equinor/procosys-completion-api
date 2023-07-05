using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Links;

public record CreateLinkDto(
    [Required]
    string Title,
    [Required]
    string Url);
