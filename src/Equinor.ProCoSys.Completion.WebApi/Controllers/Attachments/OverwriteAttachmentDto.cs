using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Attachments;

// made this Dto as class and not record to keep it testable via the abstract base class
public class OverwriteAttachmentDto : UploadBaseDto
{
    [Required]
    public string RowVersion { get; set; } = null!;
}
