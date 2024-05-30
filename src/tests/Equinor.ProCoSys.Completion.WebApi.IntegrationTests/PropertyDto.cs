#nullable enable
namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public record PropertyDto(
    string Name,
    string? OldValue,
    string? Value,
    string ValueDisplayType);
