using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Links;

public record UpdateLinkDto(
    [Required]
    string Title,
    [Required]
    string Url,
    [Required]
    string RowVersion);
