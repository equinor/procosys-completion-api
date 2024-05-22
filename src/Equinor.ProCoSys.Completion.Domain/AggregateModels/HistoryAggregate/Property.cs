using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;

// todo add unit tests
public class Property : EntityBase
{
    public const int NameLengthMax = 128;
    public const int ValueDisplayTypeLengthMax = 64;

    public Property(string name, string? oldValue, string? currentValue, string valueDisplayType)
    {
        Name = name;
        OldValue = oldValue;
        CurrentValue = currentValue;
        ValueDisplayType = valueDisplayType;
    }

    public string Name { get; private set; }
    public string? OldValue { get; private set; }
    public string? CurrentValue { get; private set; }
    public string ValueDisplayType { get; private set; }
}
