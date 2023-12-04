namespace Equinor.ProCoSys.Completion.MessageContracts;

public interface IProperty
{
    string Name { get; }
    object? OldValue { get; }
    object? NewValue { get; }
}
