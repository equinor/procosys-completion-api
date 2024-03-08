using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.WebApi.Controllers.Attachments;
using Equinor.ProCoSys.Completion.WebApi.Controllers.Comments;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;

public record CreatePunchItemDto(
    [Required] Category Category,
    [Required] string Description,
    [Required] Guid ProjectGuid,
    [Required] Guid CheckListGuid,
    [Required] Guid RaisedByOrgGuid,
    [Required] Guid ClearingByOrgGuid,
    Guid? ActionByPersonOid,
    DateTime? DueTimeUtc,
    Guid? PriorityGuid,
    Guid? SortingGuid,
    Guid? TypeGuid,
    int? Estimate,
    Guid? OriginalWorkOrderGuid,
    Guid? WorkOrderGuid,
    Guid? SWCRGuid,
    Guid? DocumentGuid,
    string? ExternalItemNo,
    bool MaterialRequired,
    DateTime? MaterialETAUtc,
    string? MaterialExternalNo,
    CreateCommentDto[]? Comments,
    UploadNewAttachmentDto[]? Attachements
    );



