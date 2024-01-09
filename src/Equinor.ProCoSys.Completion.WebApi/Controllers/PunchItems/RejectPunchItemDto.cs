using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;

public record RejectPunchItemDto(
    [Required]
    string Comment,
    [Required]
    List<Guid> Mentions,
    [Required]
    string RowVersion);
