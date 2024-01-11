using System;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;

namespace Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;

public record HistoryDeletedIntegrationEvent(
    string Plant,
    string DisplayName,
    Guid Guid,
    Guid? ParentGuid,
    User EventBy,
    DateTime EventAtUtc) : IHistoryItemDeletedV1;
