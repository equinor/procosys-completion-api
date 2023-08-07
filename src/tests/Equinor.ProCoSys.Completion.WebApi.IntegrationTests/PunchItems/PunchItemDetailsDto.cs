using System;
using JetBrains.Annotations;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.PunchItems;

public record PunchItemDetailsDto(
    Guid Guid,
    string ProjectName,
    int ItemNo,
    string Description,
    PersonDto CreatedBy,
    DateTime CreatedAtUtc,
    // todo 104514 Enable Nullable in test projects?
    [CanBeNull]
    PersonDto ModifiedBy,
    DateTime? ModifiedAtUtc,
    bool IsReadyToBeCleared,
    bool IsReadyToBeUncleared,
    [CanBeNull]
    PersonDto ClearedBy,
    DateTime? ClearedAtUtc,
    bool IsReadyToBeVerified,
    bool IsReadyToBeUnverified,
    bool IsReadyToBeRejected,
    [CanBeNull]
    PersonDto RejectedBy,
    DateTime? RejectedAtUtc,
    [CanBeNull]
    PersonDto VerifiedBy,
    DateTime? VerifiedAtUtc,
    string RowVersion);
