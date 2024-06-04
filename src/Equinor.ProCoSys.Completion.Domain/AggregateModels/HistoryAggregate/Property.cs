using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;

public class Property : EntityBase
{
    public const int NameLengthMax = 128;
    public const int ValueLengthMax = 4000;
    public const int ValueDisplayTypeLengthMax = 64;

    public Property(string name, string valueDisplayType)
    {
        Name = name;
        ValueDisplayType = valueDisplayType;
    }

    public string Name { get; private set; }
    public string ValueDisplayType { get; private set; }

    public string? OldValue { get; set; }
    public string? Value { get; set; }

    // Set if OldValue represent a user. For GDPR purposes if we need to remove
    public Guid? OldOidValue { get; set; }
    // Set if Value represent a user. For GDPR purposes if we need to remove
    public Guid? OidValue { get; set; }
}
