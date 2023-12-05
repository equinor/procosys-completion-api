using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public record AttachmentDto(
    Guid ParentGuid,
    Guid Guid,
    string FullBlobPath,
    string FileName,
    string Description,
    List<string> Labels,
    PersonDto CreatedBy,
    DateTime CreatedAtUtc,
    PersonDto ModifiedBy,
    DateTime? ModifiedAtUtc,
    string RowVersion);

