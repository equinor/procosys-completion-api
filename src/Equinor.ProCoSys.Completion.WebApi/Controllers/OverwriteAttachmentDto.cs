using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers;

public class OverwriteAttachmentDto : UploadBaseDto
{
    [Required]
    public string RowVersion { get; set; } = null!;
}
