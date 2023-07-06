using System;
using JetBrains.Annotations;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.PunchItems;

public record PunchItemDetailsDto(
    Guid Guid,
    string ProjectName,
    string ItemNo,
    string Description,
    PersonDto CreatedBy,
    DateTime CreatedAtUtc,
    // todo 104514 Enable Nullable in test projects?
    [CanBeNull]
    PersonDto ModifiedBy,
    DateTime? ModifiedAtUtc,
    bool IsReadyToBeCleared,
    [CanBeNull]
    PersonDto ClearedBy,
    DateTime? ClearedAtUtc,
    string RowVersion);
