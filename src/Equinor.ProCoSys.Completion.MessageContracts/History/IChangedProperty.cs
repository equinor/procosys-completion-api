namespace Equinor.ProCoSys.Completion.MessageContracts.History;

public interface IChangedProperty
{
    string Name { get; }
    object? OldValue { get; }
    object? NewValue { get; }
    ValueDisplayType ValueDisplayType { get; }
}
