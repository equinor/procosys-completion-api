namespace Equinor.ProCoSys.Completion.MessageContracts.History;

// todo 109354 to be renamed to IChangedProperty. Current INewProperty should then be renamed to IProperty
public interface IProperty
{
    string Name { get; }
    object? OldValue { get; }
    object? NewValue { get; }
}
