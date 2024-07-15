using System;

namespace Equinor.ProCoSys.Completion.Domain.Imports;

public sealed record PunchItemImportMessage(
    Guid? Guid,
    string Plant,
    string ProjectName,
    string TagNo,
    string ExternalPunchItemNo,
    string FormType,
    Optional<string?> PunchClass,
    Optional<string?> PunchItemNo,
    Optional<string?> Description,
    Optional<string?> Responsible,
    Optional<string?> RaisedByOrganization,
    Optional<string?> Status,
    Optional<string?> PunchListType,
    Optional<DateTime?> DueDate,
    Optional<DateTime?> ClearedDate,
    Optional<string?> ClearedBy,
    Optional<string?> ClearedByOrganization,
    Optional<DateTime?> VerifiedDate,
    Optional<string?> VerifiedBy,
    Optional<DateTime?> RejectedDate,
    Optional<string?> RejectedBy,
    Optional<string?> MaterialRequired,
    Optional<DateTime?> MaterialEta,
    Optional<string?> MaterialNo
    );
