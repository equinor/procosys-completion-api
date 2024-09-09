using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.Imports;

namespace Equinor.ProCoSys.Completion.TieImport;

public sealed record PunchItemImportMessage(
    Guid MessageGuid,
    string ProjectName,
    string Plant,
    string TagNo,
    string ExternalPunchItemNo,
    string FormType,
    string Responsible,
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
            MessageGuid,
            Method,
            ProjectName,
            Plant,
            message
        );


    public readonly string Method = "CREATE";
}
