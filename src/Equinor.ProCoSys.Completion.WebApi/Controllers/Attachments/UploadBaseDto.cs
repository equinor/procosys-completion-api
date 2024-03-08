using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Attachments;

// made this Dto as class and not record to keep it testable in UploadBaseDtoValidatorTests
public abstract class UploadBaseDto
{
    [Required]
    public IFormFile File { get; set; } = null!;
    public string? Description { get; set; }
}
