using Equinor.ProCoSys.Completion.MessageContracts.History;

namespace Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;

// the generic param T is to force same type for OldValue and CurrentValue
public record ChangedProperty<T>(
    string Name,
    T? OldValue,
    T? CurrentValue,
    ValueDisplayType ValueDisplayType = ValueDisplayType.StringAsText) : IChangedProperty
{
    object? IChangedProperty.OldValue => OldValue;
    object? IChangedProperty.CurrentValue => CurrentValue;
}
