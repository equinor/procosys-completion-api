namespace Equinor.ProCoSys.Completion.Query.History;

public record PropertyDto(
    string Name,
    string? OldValue,
    string? Value,
    string ValueDisplayType);
