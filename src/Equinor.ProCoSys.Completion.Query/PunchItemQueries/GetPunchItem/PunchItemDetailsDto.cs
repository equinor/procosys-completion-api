using System;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItem;

public record PunchItemDetailsDto(
    Guid Guid,
    string ProjectName,
    string ItemNo,
    string? Description,
    PersonDto CreatedBy,
    DateTime CreatedAtUtc,
    PersonDto? ModifiedBy,
    DateTime? ModifiedAtUtc,
    bool IsReadyToBeCleared,
    PersonDto? ClearedBy,
    DateTime? ClearedAtUtc,
    bool IsReadyToBeVerified,
    PersonDto? VerifiedBy,
    DateTime? VerifiedAtUtc,
    string RowVersion);
