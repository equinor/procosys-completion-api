using System;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public record AttachmentDto(
    Guid SourceGuid,
    Guid Guid,
    string FullBlobPath,
    string FileName,
    PersonDto CreatedBy,
    DateTime CreatedAtUtc,
    PersonDto ModifiedBy,
    DateTime? ModifiedAtUtc,
    string RowVersion);

