using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;

public class HistoryItem : EntityBase, IAggregateRoot
{
    public const int EventDisplayNameLengthMax = 254;
    public const int EventByFullNameLengthMax = 64;

    private readonly List<Property> _properties = new();

    public HistoryItem(
        Guid eventForGuid,
        string eventDisplayName,
        Guid eventByOid, 
        string eventByFullName, 
        DateTime eventAtUtc, 
        Guid? eventForParentGuid = null)
    {
        EventForGuid = eventForGuid;
        EventDisplayName = eventDisplayName;
        EventByOid = eventByOid;
        EventByFullName = eventByFullName;
        if (eventAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new Exception($"{nameof(HistoryItem)}.{nameof(EventAtUtc)} must be UTC");
        }
        EventAtUtc = eventAtUtc;
        EventForParentGuid = eventForParentGuid;
    }

    public IReadOnlyCollection<Property> Properties => _properties.AsReadOnly();

    // private setters needed for Entity Framework
    public Guid EventForGuid { get; private set; }
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string EventDisplayName { get; private set; }
    public Guid EventByOid { get; private set; }
    public DateTime EventAtUtc { get; private set; }
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string EventByFullName { get; private set; }
    public Guid? EventForParentGuid { get; private set; }
    public void AddProperty(Property property) => _properties.Add(property);
}
