using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.MessageContracts;

namespace Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;

public class PunchItemCategoryUpdatedDomainEvent : PunchItemDomainEvent
{
    public PunchItemCategoryUpdatedDomainEvent(PunchItem punchItem, IProperty change) : base(punchItem) 
        => Change = change;

    public IProperty Change { get; }
}
