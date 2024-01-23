using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;

namespace Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;

public record HistoryUpdatedIntegrationEvent(
    string Plant,
    string DisplayName,
    Guid Guid,
    User EventBy,
    DateTime EventAtUtc,
    List<IChangedProperty> ChangedProperties) : IHistoryItemUpdatedV1;
