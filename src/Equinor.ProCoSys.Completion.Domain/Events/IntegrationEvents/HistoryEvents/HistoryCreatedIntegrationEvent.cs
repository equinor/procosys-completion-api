using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using MassTransit;

namespace Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;

public record HistoryCreatedIntegrationEvent(
    string DisplayName,
    Guid Guid,
    Guid? ParentGuid,
    User EventBy,
    DateTime EventAtUtc,
    List<IProperty> Properties) : IHistoryItemCreatedV1
{
    public Guid MessageId { get; }  = NewId.NextGuid();
}
