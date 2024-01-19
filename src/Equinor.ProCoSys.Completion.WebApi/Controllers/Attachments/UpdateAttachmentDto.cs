using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Attachments;

public record UpdateAttachmentDto(
    [Required]
    string Description,
    [Required]
    List<string> Labels,
    [Required]
    string RowVersion);
