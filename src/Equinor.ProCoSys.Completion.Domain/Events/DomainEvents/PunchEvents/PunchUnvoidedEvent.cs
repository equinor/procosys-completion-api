using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using MediatR;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;

public class PunchUnvoidedEvent : INotification
{
    public PunchUnvoidedEvent(string displayName, Guid sourceGuid)
    {
        DisplayName = displayName;
        SourceGuid = sourceGuid;
    }

    public Guid SourceGuid { get; init; }
    
    public string DisplayName { get; init; }
}
