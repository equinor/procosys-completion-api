using Equinor.ProCoSys.Completion.MessageContracts;

namespace Equinor.ProCoSys.Completion.Domain.Events;

public record Property<T>(string Name, T? OldValue, T? NewValue) : IProperty
{
    object? IProperty.OldValue => OldValue;
    object? IProperty.NewValue => NewValue;
}
