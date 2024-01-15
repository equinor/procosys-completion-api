using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.PunchItem;

namespace Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.PunchItemEvents;

public record PunchItemDeletedIntegrationEvent
(
    string Plant,
    Guid Guid,
    Guid ParentGuid,
    User DeletedBy,
    DateTime DeletedAtUtc
) : IPunchItemDeletedV1
{
    public PunchItemDeletedIntegrationEvent(PunchItem punchItem) : this(
        punchItem.Plant,
        punchItem.Guid,
        punchItem.CheckListGuid,
        // Our entities don't have DeletedByOid / DeletedAtUtc ...
        // ... but both ModifiedBy and ModifiedAtUtc are updated when entity is deleted
        new User(punchItem.ModifiedBy!.Guid, punchItem.ModifiedBy!.GetFullName()),
        punchItem.ModifiedAtUtc!.Value)
    { }
}
