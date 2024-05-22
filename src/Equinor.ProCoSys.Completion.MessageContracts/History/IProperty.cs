namespace Equinor.ProCoSys.Completion.MessageContracts.History;

public interface IProperty
{
    string Name { get; }
    object? CurrentValue { get; }
    ValueDisplayType ValueDisplayType { get; }
}
