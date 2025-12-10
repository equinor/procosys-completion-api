using Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportPunch;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.TieImport.Models;

public sealed record PunchItemImportMessage(
    Guid MessageGuid,
    string Method,
    string ProjectName,
    string Plant,
    string TagNo,
    string ExternalPunchItemNo,
    string FormType,
    string Responsible,
    Optional<long?> PunchItemNo,
    Optional<string?> Description,
    Optional<string?> RaisedByOrganization,
    Category? Category,
    OptionalWithNull<string?> PunchListType,
    OptionalWithNull<DateTime?> DueDate,
    Optional<DateTime?> ClearedDate,
    Optional<string?> ClearedBy,
    Optional<string?> ClearedByOrganization,
    Optional<DateTime?> VerifiedDate,
    Optional<string?> VerifiedBy,
    Optional<DateTime?> RejectedDate,
    Optional<string?> RejectedBy,
    Optional<bool?> MaterialRequired,
    OptionalWithNull<DateTime?> MaterialEta,
    OptionalWithNull<string?> MaterialNo,
    OptionalWithNull<string?> ActionBy,
    OptionalWithNull<string?> DocumentNo,
    OptionalWithNull<int?> Estimate,
    OptionalWithNull<string?> OriginalWorkOrderNo,
    OptionalWithNull<string?> WorkOrderNo,
    OptionalWithNull<string?> Priority,
    OptionalWithNull<string?> Sorting,
    OptionalWithNull<int?> SwcrNo,
    Optional<string?> IsVoided
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
}
