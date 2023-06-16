using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers;

public abstract class UploadBaseDto
{
    [Required]
    public IFormFile File { get; set; } = null!;
}
