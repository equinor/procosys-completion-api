using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.Query.Attachments;

public record AttachmentDto(
    Guid ParentGuid,
    Guid Guid,
    string FullBlobPath,
    string FileName,
    string Description,
    List<string> Labels,
    PersonDto CreatedBy,
    DateTime CreatedAtUtc,
    PersonDto? ModifiedBy,
    DateTime? ModifiedAtUtc,
    string RowVersion);
