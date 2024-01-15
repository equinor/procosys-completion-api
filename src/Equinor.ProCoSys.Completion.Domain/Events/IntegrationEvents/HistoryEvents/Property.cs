using Equinor.ProCoSys.Completion.MessageContracts.History;

namespace Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;

// todo 109354 to be renamed to ChangedProperty. Current NewProperty should then be renamed to Property
// the generic param T is to force same type for OldValue and NewValue
public record Property<T>(string Name, T? OldValue, T? NewValue) : IProperty
{
    object? IProperty.OldValue => OldValue;
    object? IProperty.NewValue => NewValue;
}
