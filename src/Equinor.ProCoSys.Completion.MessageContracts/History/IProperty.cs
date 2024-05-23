namespace Equinor.ProCoSys.Completion.MessageContracts.History;

public interface IProperty
{
    string Name { get; }
    object? Value { get; }
    ValueDisplayType ValueDisplayType { get; }
}
