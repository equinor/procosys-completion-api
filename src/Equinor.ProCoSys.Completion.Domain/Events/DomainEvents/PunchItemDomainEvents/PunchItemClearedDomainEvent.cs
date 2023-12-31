﻿using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;

public class PunchItemClearedDomainEvent : PunchItemDomainEvent
{
    public PunchItemClearedDomainEvent(PunchItem punchItem) : base(punchItem)
    { }
}
