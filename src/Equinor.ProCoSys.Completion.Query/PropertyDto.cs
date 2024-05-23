namespace Equinor.ProCoSys.Completion.Query;

public record PropertyDto(
    string Name,
    string? OldValue,
    string? Value,
    string ValueDisplayType);
