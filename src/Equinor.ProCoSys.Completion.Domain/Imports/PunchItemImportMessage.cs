using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Imports;

public sealed record PunchItemImportMessage(
    Guid? Guid,
    string Plant,
    string Method,
    string ProjectName,
    string TagNo,
    string ExternalPunchItemNo,
    string FormType,
    string Responsible,
    Optional<string?> PunchClass,
    Optional<string?> PunchItemNo,
    Optional<string?> Description,
    Optional<string?> RaisedByOrganization,
    Category? Category,
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
)
{
    public ImportError ToImportError(string message) =>
        new(
            Guid,
            Method,
            ProjectName,
            Plant,
            message
        );
};
