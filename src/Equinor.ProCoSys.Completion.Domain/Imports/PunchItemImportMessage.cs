namespace Equinor.ProCoSys.Completion.Domain.Imports;

public sealed record PunchItemImportMessage(
    Optional<string?> PunchClass,
    Optional<string?> ExternalPunchItemNo,
    Optional<string?> TagNo,
    Optional<string?> PunchItemNo,
    Optional<string?> Description);
