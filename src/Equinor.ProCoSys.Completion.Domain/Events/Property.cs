namespace Equinor.ProCoSys.Completion.Domain.Events;

public record Property(string Name, object? OldValue, object? NewValue);
