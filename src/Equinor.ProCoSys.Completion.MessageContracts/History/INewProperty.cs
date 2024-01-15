namespace Equinor.ProCoSys.Completion.MessageContracts.History;

// todo 109354 to be renamed to IProperty after current IProperty is renamed to IChangedProperty
public interface INewProperty
{
    string Name { get; }
    object? Value { get; }
}
