using System.ComponentModel.DataAnnotations;
using System;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;

public record DuplicatePunchItemDto
(
    [Required] Guid[] CheckListGuids,
    [Required] bool IncludeAttachments
);
