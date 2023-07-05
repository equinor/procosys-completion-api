using System;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.PunchItems;

public record PunchItemDetailsDto(
    Guid Guid,
    string ProjectName,
    string ItemNo,
    string Description,
    PersonDto CreatedBy,
    DateTime CreatedAtUtc,
    PersonDto ModifiedBy,
    DateTime? ModifiedAtUtc,
    bool IsReadyToBeCleared,
    PersonDto ClearedBy,
    DateTime? ClearedAtUtc,
    string RowVersion);
