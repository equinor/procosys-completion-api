using Equinor.ProCoSys.Completion.MessageContracts.History;

namespace Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;

// the generic param T is to force same type for OldValue and NewValue
public record ChangedProperty<T>(
    string Name,
    T? OldValue,
    T? NewValue,
    ValueDisplayType ValueDisplayType = ValueDisplayType.StringAsText) : IChangedProperty
{
    object? IChangedProperty.OldValue => OldValue;
    object? IChangedProperty.NewValue => NewValue;
}
