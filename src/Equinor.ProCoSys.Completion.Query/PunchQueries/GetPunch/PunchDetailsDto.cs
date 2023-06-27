using System;

namespace Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunch;

public record PunchDetailsDto(
    Guid Guid,
    string ProjectName,
    string ItemNo,
    string? Description,
    PersonDto CreatedBy,
    DateTime CreatedAtUtc,
    PersonDto? ModifiedBy,
    DateTime? ModifiedAtUtc,
    string RowVersion);
