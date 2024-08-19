using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;

public record DuplicatePunchItemDto(
    [Required] List<Guid> CheckListGuids,
    [Required] bool DuplicateAttachments);
