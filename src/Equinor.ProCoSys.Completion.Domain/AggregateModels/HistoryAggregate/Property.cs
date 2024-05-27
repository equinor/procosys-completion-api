using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;

// todo History: Add unit tests
public class Property : EntityBase
{
    public const int NameLengthMax = 128;
    public const int ValueDisplayTypeLengthMax = 64;

    public Property(string name, string? oldValue, string? value, string valueDisplayType)
    {
        Name = name;
        OldValue = oldValue;
        Value = value;
        ValueDisplayType = valueDisplayType;
    }

    public string Name { get; private set; }
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? OldValue { get; private set; }
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? Value { get; private set; }
    public string ValueDisplayType { get; private set; }
}
